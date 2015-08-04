namespace Magicalia.MPS.Utilities
{
    public interface IMfmWebClient
    {
        ClientResult GetResponse(string url, string authHeader);
    }
}
