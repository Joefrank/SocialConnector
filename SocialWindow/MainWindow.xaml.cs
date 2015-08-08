using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autofac;
using SocialConnector.Services.Infrastructure;
using SocialConnector.Services.Implementation;
using SocialConnector.Services;
using SocialConnector.Utils;
using Autofac.Features.Indexed;
using System.Configuration;

namespace SocialWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IContainer container;

        public IIndex<SocialSite, ISocialService> SocialServiceFactory { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            RegisterTypes();
        }

        private void GetTweetByUrlOrId(object sender, RoutedEventArgs e)
        {   
            var inputDialog = new InputDialog("Please enter link or id of tweet:");

            if (inputDialog.ShowDialog() == true)
            {
                var input = inputDialog.Answer;
                
                if (string.IsNullOrEmpty(input))
                {
                    MessageBox.Show("You need to enter a Url or Id for the tweet you want!");
                    return;
                }

               
                var service = container.ResolveKeyed<ISocialService>(SocialSite.Twitter);

                //var socialService = SocialServiceFactory[SocialSite.Twitter];
                var html = service.GetHtml(input);

                //browserDisplay.NavigateToString("<html><head></head><body>First row<br>" + input + " Second row</body></html>");

                browserDisplay.NavigateToString(html);
            }
        }

        public void RegisterTypes()
        {
            var builder = new ContainerBuilder();
            //builder.RegisterType<TwitterService>()
            //   .Keyed<BaseSocialService>(SocialSite.Twitter)
            //   .InstancePerDependency();
            
            //builder.RegisterType<VineService>()
            //   .Keyed<BaseSocialService>(SocialSite.Vine)
            //   .InstancePerDependency();

            //builder.RegisterType<InstagramService>()
            //   .Keyed<BaseSocialService>(SocialSite.Instagram)
            //   .InstancePerDependency();

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
                .WithParameter("customParameters",twitterDico) ;

            container = builder.Build();

           
        }
       
    }
}
