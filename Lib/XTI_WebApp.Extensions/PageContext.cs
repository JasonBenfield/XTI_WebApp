using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;

namespace XTI_WebApp.Extensions
{
    public sealed class PageContext
    {
        private readonly WebAppOptions webAppOptions;
        private readonly CacheBust cacheBust;

        public PageContext(IOptions<WebAppOptions> webAppOptions, CacheBust cacheBust)
        {
            this.webAppOptions = webAppOptions.Value;
            this.cacheBust = cacheBust;
        }

        public string BaseUrl { get; private set; }
        public string CacheBust { get; private set; }

        public async Task<string> Serialize()
        {
            BaseUrl = string.IsNullOrWhiteSpace(webAppOptions.BaseUrl) ? "/" : webAppOptions.BaseUrl;
            CacheBust = await cacheBust.Value();
            return JsonSerializer.Serialize(this);
        }
    }
}
