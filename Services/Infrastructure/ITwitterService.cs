using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialConnector.Services.Infrastructure
{
    public interface ITwitterService : ISocialService
    {
        List<Status> GetMostRecentPostFromHome(int noOfPost);

        List<Status> SearchTwitter(string searchTerm, int noOfRecords);

        List<string> GetFollowers(int noOfFollowers = 50);

        Task<Status> SendTweet(string tweetText);
    }
}
