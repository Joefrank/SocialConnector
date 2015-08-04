using Magicalia.MPS.Infrastructure.Settings;
using Magicalia.MPS.Utilities;

namespace Magicalia.MPS.Modules.Site.Services
{
    public class InstagramService : BaseSocialService, ISocialService
    {
        public InstagramService(IMfmWebClient webClient, ICustomParameters customParameters)
            : base(webClient)
        {
            Endpoint = customParameters.GetCustomParameter("InstagramEmbedUrl");
        }
    }
}