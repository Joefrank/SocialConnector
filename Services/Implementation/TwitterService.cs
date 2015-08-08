using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SocialConnector.Services.Infrastructure;
using SocialConnector.Utils;
using System.Collections.Generic;
using System.Linq;
using LinqToTwitter;
using System.Threading.Tasks;

namespace SocialConnector.Services.Implementation
{
    public class TwitterService : BaseSocialService, ITwitterService
    {
        private const string OauthVersion = "1.0";
        private const string OauthSignatureMethod = "HMAC-SHA1";

        private const string OauthQueryStringFormat =
            "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";

        private const string OauthHeaderFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                                 "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                                 "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                                 "oauth_version=\"{6}\"";

        private readonly string _oauthConsumerKey;
        private readonly string _oauthConsumerSecret;
        private readonly string _oauthToken;
        private readonly string _oauthTokenSecret;

        private string _oauthNonce;
        private string _oauthTimestamp;

        private SingleUserAuthorizer _authorizer;

        public TwitterService(IGenericWebClient webClient, Dictionary<string,string> customParameters)
            : base(webClient)
        {
            Endpoint = customParameters["TwitterEmbedUrl"];
            _oauthConsumerKey = customParameters["TwitterConsumerKey"];
            _oauthConsumerSecret = customParameters["TwitterConsumerSecret"];
            _oauthToken = customParameters["TwitterAccessToken"];
            _oauthTokenSecret = customParameters["TwitterAccessTokenSecret"];

            _authorizer =  new SingleUserAuthorizer
              {
                 CredentialStore =  new SingleUserInMemoryCredentialStore
                 {
                     ConsumerKey = _oauthConsumerKey,
                     ConsumerSecret = _oauthConsumerSecret,
                     AccessToken = _oauthToken,
                     AccessTokenSecret = _oauthTokenSecret
                 }
              };

        }

        public override string GetHtml(string queryStringParamValue)
        {
            if (string.IsNullOrEmpty(Endpoint))
            {
                return null;
            }

            // Build final customisation parameter
            var parameterName = GetParamNameFromValue(queryStringParamValue);
            var identParam = string.Format(
                "{0}={1}",
                parameterName,
                Uri.EscapeDataString(queryStringParamValue));

            // Generate nonce and timestamp
            PopulateUniqueRequestParameters();

            // Parameters need id prefix or url suffix
            //  otherwise the OAuth signature will be invalid
            var parameters = BuildParameters();
            if (parameterName.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                parameters = identParam + "&" + parameters;
            }
            else
            {
                parameters = parameters + "&" + identParam;
            }

            // Build OAuth signature from the associated endpoint and parameters
            // NOTE: Endpoint URI without id=39392 or url=http://
            var oAuthSignature = BuildOauthSignature(Endpoint, parameters);

            // Generate authorization header needed for web-request
            var authHeader = BuildAuthHeader(oAuthSignature);
            
            // Generate final request URL with the ident parameter as a query
            var uriBuilder = new UriBuilder(Endpoint);
            uriBuilder.Query += identParam;
            var url = uriBuilder.Uri.AbsoluteUri;

            // Get the response HTML from Twitter API and return
            return GetResponseHtml(url, authHeader);
        }

        public void PopulateUniqueRequestParameters()
        {
            // Get tje current time as base64 string
            _oauthNonce = Convert.ToBase64String(Encoding.ASCII.GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            _oauthTimestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        public string BuildParameters()
        {
            // unique request details
            return string.Format(OauthQueryStringFormat,
                _oauthConsumerKey,
                _oauthNonce,
                OauthSignatureMethod,
                _oauthTimestamp,
                _oauthToken,
                OauthVersion);
        }

        public string BuildOauthSignature(string url, string parameters)
        {
            var baseString = string.Concat("GET&", Uri.EscapeDataString(url), "&", Uri.EscapeDataString(parameters));

            var compositeKey = string.Concat(Uri.EscapeDataString(_oauthConsumerSecret), "&",
                Uri.EscapeDataString(_oauthTokenSecret));

            string oauthSignature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
            {
                oauthSignature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
            }

            return oauthSignature;
        }

        public string BuildAuthHeader(string oauthSignature)
        {
            return string.Format(OauthHeaderFormat,
                Uri.EscapeDataString(_oauthNonce),
                Uri.EscapeDataString(OauthSignatureMethod),
                Uri.EscapeDataString(_oauthTimestamp),
                Uri.EscapeDataString(_oauthConsumerKey),
                Uri.EscapeDataString(_oauthToken),
                Uri.EscapeDataString(oauthSignature),
                Uri.EscapeDataString(OauthVersion)
                );
        }

        /// <summary>
        /// Should not exceed 200 otherwise, use loop
        /// </summary>
        /// <param name="noOfPost"></param>
        /// <returns></returns>
        public List<Status> GetMostRecentPostFromHome(int noOfPost)
        {
            try
            {
                var twitterContext = new TwitterContext(_authorizer);

                var tweets = from tweet in twitterContext.Status
                             where tweet.Type == StatusType.Home &&
                             tweet.Count == noOfPost
                             select tweet;

                return tweets.ToList();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a list of followers form authorized user
        /// </summary>
        /// <param name="noOfFollowers"></param>
        /// <returns></returns>
        public List<string> GetFollowers(int noOfFollowers = 50)
        {
            var results = new List<string>();

            var twitterContext = new TwitterContext(_authorizer);

            var temp = Enumerable.FirstOrDefault(from friend in
                                                     twitterContext.Friendship
                                                 where friend.Type == FriendshipType.FollowersList &&
                                                    friend.ScreenName == "joe_bolla" &&
                                                    friend.Count == noOfFollowers
                                                 select friend);

            if (temp != null)
            {
                temp.Users.ToList().ForEach(user => results.Add(user.Name));

                while (temp != null && temp.CursorMovement.Next != 0)
                {
                    temp = Enumerable.FirstOrDefault(from friend in
                                                         twitterContext.Friendship
                                                     where friend.Type == FriendshipType.FollowersList &&
                                                        friend.ScreenName == "joe_bolla" &&
                                                        friend.Count == noOfFollowers &&
                                                        friend.Cursor == temp.CursorMovement.Next
                                                     select friend);

                    if (temp != null) temp.Users.ToList().ForEach(user =>
                       results.Add(user.Name));
                }
            }

            return results;
        }


        /// <summary>
        /// Gets most recent posts and then count tweets and update the list of inputNames provided
        /// </summary>
        /// <param name="inputNames"></param>
        /// <returns></returns>
        private List<string> GetSideBarList(List<string> inputNames)
        {
           var results = new List<string>();
           var currentTweets = GetMostRecentPostFromHome(200);

           foreach (string name in inputNames)
           {
              int tweetCount = currentTweets.Count(tweet =>
                 tweet.User.Name == name);
              if(tweetCount > 0)
              {
                 results.Add(string.Format("{0} ({1})",
                    name, tweetCount));
              }
              else
              {
                 results.Add(string.Format("{0}", name));
              }
           }
 
           return results;
        }

        /// <summary>
        /// Searches tweeter using a term max statuses returned is 200
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public List<Status> SearchTwitter(string searchTerm, int noOfRecords)
        {
            var twitterContext = new TwitterContext(_authorizer);
           
            var srch =
               Enumerable.SingleOrDefault((from search in
                                               twitterContext.Search
                                           where search.Type == SearchType.Search &&
                                              search.Query == searchTerm &&
                                              search.Count == noOfRecords
                                           select search));
            if (srch != null && srch.Statuses.Count > 0)
            {
                return srch.Statuses.ToList();
            }

            return new List<Status>();
        }

        public async Task<Status> SendTweet(string tweetText)
        {
            using (var twitterCtx = new TwitterContext(_authorizer))
            {
                var tweet = await twitterCtx.TweetAsync(tweetText); // error here

                return tweet;
            }
        }
 
    }
}