using System;
using Magicalia.MPS.Infrastructure.Settings;
using Magicalia.MPS.Utilities;

namespace Magicalia.MPS.Modules.Site.Services
{
    public class VineService : BaseSocialService, ISocialService
    {
        private const string UserAgentHeader =
            "com.vine.iphone/1.0.3 (unknown, iPhone OS 6.0.1, iPhone, Scale/2.000000)";

        private const string AcceptLanguageHeader =
            "en, sv, fr, de, ja, nl, it, es, pt, pt-PT, da, fi, nb, ko, zh-Hans, zh-Hant, ru, pl, tr, uk, ar, hr, cs, el, he, ro, sk, th, id, ms, en-GB, ca, hu, vi, en-us;q=0.8";

        private const string HeaderFormat = "user-agent=\"{0}\", accept-language=\"{1}\"";

        public VineService(IMfmWebClient webClient, ICustomParameters customParameters)
            : base(webClient)
        {
            Endpoint = customParameters.GetCustomParameter("VineEmbedUrl");
            AuthHeader = string.Format(HeaderFormat,
                                       Uri.EscapeDataString(UserAgentHeader),
                                       Uri.EscapeDataString(AcceptLanguageHeader)
                );
        }
    }
}