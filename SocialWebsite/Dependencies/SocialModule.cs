using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;

using SocialConnector.Services.Infrastructure;
using SocialConnector.Services.Implementation;
using SocialConnector.Utils;
using System.Configuration;


namespace SocialWebsite.Dependencies
{
    public class SocialModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var twitterDico = new Dictionary<string, string>();

            twitterDico["TwitterEmbedUrl"] = ConfigurationManager.AppSettings["TwitterEmbedUrl"];
            twitterDico["TwitterConsumerKey"] = ConfigurationManager.AppSettings["TwitterConsumerKey"];
            twitterDico["TwitterConsumerSecret"] = ConfigurationManager.AppSettings["TwitterConsumerSecret"];
            twitterDico["TwitterAccessToken"] = ConfigurationManager.AppSettings["TwitterAccessToken"];
            twitterDico["TwitterAccessTokenSecret"] = ConfigurationManager.AppSettings["TwitterAccessTokenSecret"];


            builder.RegisterType<GenericWebClient>().As<IGenericWebClient>();
            builder.RegisterType<InstagramService>().Keyed<ISocialService>(SocialSite.Instagram);
            builder.RegisterType<VineService>().Keyed<ISocialService>(SocialSite.Vine);
            builder.RegisterType<TwitterService>().Keyed<ISocialService>(SocialSite.Twitter).
                WithParameter("webClient", new GenericWebClient())
                .WithParameter("customParameters", twitterDico);

            base.Load(builder);
        }
    }
}