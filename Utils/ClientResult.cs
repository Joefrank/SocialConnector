using System.Net;
using Newtonsoft.Json.Linq;

namespace SocialConnector.Utils
{
    public class ClientResult
    {
        public bool CompletedWithoutErrors { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public JObject CompleteResponse { get; set; }

        public string ResponseHtml
        {
            get
            {
                if (CompleteResponse != null && CompleteResponse["html"] != null)
                {
                    return CompleteResponse["html"].ToString();
                }

                return null;
            }
        }
    }
}