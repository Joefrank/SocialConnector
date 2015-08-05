using SocialConnector.Utils;
using SocialConnector.Services.Infrastructure;

namespace SocialConnector.Services.Implementation
{
    public class InstagramService : BaseSocialService, ISocialService
    {
        public InstagramService(IGenericWebClient webClient, string endPoint)
            : base(webClient)
        {
            Endpoint = endPoint;
        }
    }
}