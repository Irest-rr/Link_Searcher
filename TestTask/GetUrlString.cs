using System.Net;

namespace TestTask
{
    public static class GetUrlString
    {
        public static string GetUrl(this string address)
        {
            WebClient client = new WebClient();

            client.Credentials = CredentialCache.DefaultNetworkCredentials;

            return client.DownloadString(address);
        }
    }
}
