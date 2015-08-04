using System;
using System.IO;
using System.Net;
using Magicalia.MPS.Utilities.Logging;
using Newtonsoft.Json.Linq;

namespace Magicalia.MPS.Utilities
{
    public class MfmWebClient : IMfmWebClient
    {
        private readonly ILogger<IMfmWebClient> _logger;

        public MfmWebClient(ILogger<IMfmWebClient> logger)
        {
            _logger = logger;
        }

        public ClientResult GetResponse(string url, string authHeader)
        {
            try
            {
                var request = (HttpWebRequest) WebRequest.Create(url);
                request.Headers.Add("Authorization", authHeader);
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";

                var response = (HttpWebResponse) request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                var objText = reader.ReadToEnd();
                var js = JObject.Parse(objText);

                return new ClientResult
                    {
                        CompletedWithoutErrors = true,
                        HttpStatusCode = response.StatusCode,
                        CompleteResponse = js
                    };
            }
            catch (Exception e)
            {
                _logger.Warn(String.Format("Unexpected error occured in getting response: {0}", Environment.NewLine), e);

                return new ClientResult
                    {
                        CompletedWithoutErrors = false
                    };
            }
        }
    }
}