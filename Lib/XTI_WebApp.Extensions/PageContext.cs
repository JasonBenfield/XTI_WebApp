using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Extensions
{
    public sealed class PageContext
    {
        private readonly AppOptions appOptions;
        private readonly CacheBust cacheBust;
        private readonly IAppContext appContext;
        private readonly IHostEnvironment hostEnvironment;

        public PageContext(IOptions<AppOptions> appOptions, CacheBust cacheBust, IAppContext appContext, IHostEnvironment hostEnvironment)
        {
            this.appOptions = appOptions.Value;
            this.cacheBust = cacheBust;
            this.appContext = appContext;
            this.hostEnvironment = hostEnvironment;
        }

        public string BaseUrl { get; private set; }
        public string CacheBust { get; private set; }
        public string AppTitle { get; private set; }
        public string EnvironmentName { get; private set; }

        public async Task<string> Serialize()
        {
            BaseUrl = string.IsNullOrWhiteSpace(appOptions.BaseUrl) ? "/" : appOptions.BaseUrl;
            CacheBust = await cacheBust.Value();
            var app = await appContext.App();
            AppTitle = app.Title;
            EnvironmentName = hostEnvironment.EnvironmentName;
            return JsonSerializer.Serialize(this);
        }
    }
}
