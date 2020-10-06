using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;

namespace XTI_WebApp.Extensions
{
    public sealed class PageContext
    {
        private readonly AppOptions appOptions;
        private readonly CacheBust cacheBust;

        public PageContext(IOptions<AppOptions> webAppOptions, CacheBust cacheBust)
        {
            this.appOptions = webAppOptions.Value;
            this.cacheBust = cacheBust;
        }

        public string BaseUrl { get; private set; }
        public string CacheBust { get; private set; }

        public async Task<string> Serialize()
        {
            BaseUrl = string.IsNullOrWhiteSpace(appOptions.BaseUrl) ? "/" : appOptions.BaseUrl;
            CacheBust = await cacheBust.Value();
            return JsonSerializer.Serialize(this);
        }
    }
}
