namespace SocialConnector.Utils
{
    public interface IGenericWebClient
    {
        ClientResult GetResponse(string url, string authHeader);
    }
}
