using System.Net.Http;

namespace XTI_WebAppClient
{
    public class AppClient
    {
        protected readonly IHttpClientFactory httpClientFactory;
        protected readonly string url;

        protected XtiToken xtiToken;

        protected AppClient(IHttpClientFactory httpClientFactory, string baseUrl, string appKey, string version)
        {
            this.httpClientFactory = httpClientFactory;
            url = $"{baseUrl}/{appKey}/{version}";
        }

        public override string ToString() => $"{GetType().Name} {url}";
    }
}
