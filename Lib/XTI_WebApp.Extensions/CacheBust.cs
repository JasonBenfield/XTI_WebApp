using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using XTI_App;

namespace XTI_WebApp.Extensions
{
    public sealed class CacheBust
    {
        private readonly WebAppOptions options;
        private readonly IHostEnvironment hostEnvironment;
        private readonly AppFactory appFactory;
        private readonly XtiPath xtiPath;

        private string value;

        public CacheBust(IOptions<WebAppOptions> options, IHostEnvironment hostEnvironment, AppFactory appFactory, XtiPath xtiPath)
        {
            this.options = options.Value;
            this.hostEnvironment = hostEnvironment;
            this.appFactory = appFactory;
            this.xtiPath = xtiPath;
        }

        public async Task<string> Value()
        {
            if (value == null)
            {
                if (string.IsNullOrWhiteSpace(options.CacheBust))
                {
                    if (hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Test"))
                    {
                        value = Guid.NewGuid().ToString("N");
                    }
                    else if (xtiPath.IsCurrentVersion())
                    {
                        var app = await appFactory.Apps().App(new AppKey(xtiPath.App));
                        var version = await app.CurrentVersion();
                        value = $"V{version.ID}";
                    }
                }
                else
                {
                    value = options.CacheBust;
                }
            }
            return value;
        }

        public async Task<string> Query() => $"cacheBust={await Value()}";

        public override string ToString() => $"{nameof(CacheBust)} {value}";
    }
}
