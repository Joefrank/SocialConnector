using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac.Features.Indexed;
using SocialConnector.Services.Implementation;
using SocialConnector.Services.Infrastructure;
using SocialWebsite.Models;
using Autofac;

namespace SocialWebsite.Controllers
{
    public class TwitterController : Controller
    {
        //private ISocialService _socialService;

        //public TwitterController(IIndex<SocialSite, ISocialService> SocialServiceFactory)
        //{
        //    _socialService = SocialServiceFactory[SocialSite.Twitter];
        //}

        private ITwitterService _socialService;

        public TwitterController(ITwitterService SocialService)
        {
            _socialService = SocialService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetTweet()
        {
            var tweetId = Request.Form["txtTweetId"];
            return GetTweet(tweetId);
        }


        public ActionResult GetTweet(string id){

            SocialStruct socialStruct;

            if (string.IsNullOrEmpty(id))
            {
                socialStruct = new SocialStruct { MissingData = true, Success = false };
            }
            else
            {

                var resultHtml = _socialService.GetHtml(id);

                if (!string.IsNullOrEmpty(resultHtml))
                {
                    socialStruct = new SocialStruct
                    {
                        HtmlResult = resultHtml,
                        Success = true,
                        MissingData = false
                    };
                }
                else
                {
                    socialStruct = new SocialStruct { MissingData = true, Success = false };
                }
            }
           
            return View("TweetDisplay", socialStruct);
        }

        public ActionResult LatestHomeTweets()
        {
            var lstPosts = _socialService.GetMostRecentPostFromHome(50);
            return View(lstPosts);
        }

        [HttpPost]
        public ActionResult LatestHomeTweets(int number)
        {
            var lstPosts = _socialService.GetMostRecentPostFromHome(10);
            return View(lstPosts);
        }

        public ActionResult PostTweet()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostTweet(string txtTweet)
        {
            var status = _socialService.SendTweet(txtTweet);
            return View(status.Status.ToString());
        }

        public ActionResult ListTweets(string topic)
        {
            

            return null;
        }



    }
}
