using System;
using System.Net;
using SocialConnector.Utils;

namespace SocialConnector.Services
{
    public abstract class BaseSocialService
    {
        private const string ParamNameUrl = "url";
        private const string ParamNameId = "id";

        private readonly IGenericWebClient _webClient;

        protected BaseSocialService(IGenericWebClient webClient)
        {
            _webClient = webClient;
        }

        public string Endpoint { get; set; }
        public string AuthHeader { get; set; }

        public virtual string GetHtml(string queryStringParamValue)
        {
            if (string.IsNullOrEmpty(Endpoint))
            {
                return null;
            }
         
            var uriBuilder = new UriBuilder(Endpoint);
            uriBuilder.Query += string.Format("{0}={1}", GetParamNameFromValue(queryStringParamValue),
                Uri.EscapeDataString(queryStringParamValue));
            var finalUrl = uriBuilder.Uri.AbsoluteUri;

            return GetResponseHtml(finalUrl, AuthHeader);
        }

        protected string GetParamNameFromValue(string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
                return "";

            if (paramValue.Trim().Contains("http://") || paramValue.Trim().Contains("https://"))
            {
                return ParamNameUrl;
            }

            return ParamNameId;
        }

        protected string GetResponseHtml(string url, string header)
        {
            var result = _webClient.GetResponse(url, header);
            if (result.CompletedWithoutErrors && result.HttpStatusCode == HttpStatusCode.OK)
            {
                return result.ResponseHtml;
            }

            return null;
        }
    }
}
