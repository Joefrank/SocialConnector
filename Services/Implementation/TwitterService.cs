using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SocialConnector.Services.Infrastructure;
using SocialConnector.Utils;
using System.Collections.Generic;


namespace SocialConnector.Services.Implementation
{
    public class TwitterService : BaseSocialService, ISocialService
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

        public TwitterService(IGenericWebClient webClient, Dictionary<string,string> customParameters)
            : base(webClient)
        {
            Endpoint = customParameters["TwitterEmbedUrl"];
            _oauthConsumerKey = customParameters["TwitterConsumerKey"];
            _oauthConsumerSecret = customParameters["TwitterConsumerSecret"];
            _oauthToken = customParameters["TwitterAccessToken"];
            _oauthTokenSecret = customParameters["TwitterAccessTokenSecret"];
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
    }
}