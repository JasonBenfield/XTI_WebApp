using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_Core.Extensions;

namespace XTI_WebApp
{
    public sealed class CacheBust
    {
        private readonly WebAppOptions options;
        private readonly IHostEnvironment hostEnvironment;
        private readonly IAppContext appContext;
        private readonly XtiPath xtiPath;

        private string value;

        public CacheBust(IOptions<WebAppOptions> options, IHostEnvironment hostEnvironment, IAppContext appContext, XtiPath xtiPath)
        {
            this.options = options.Value;
            this.hostEnvironment = hostEnvironment;
            this.appContext = appContext;
            this.xtiPath = xtiPath;
        }

        public async Task<string> Value()
        {
            if (value == null)
            {
                if (string.IsNullOrWhiteSpace(options.CacheBust))
                {
                    if (hostEnvironment.IsDevOrTest())
                    {
                        value = Guid.NewGuid().ToString("N");
                    }
                    else if (xtiPath.IsCurrentVersion())
                    {
                        var app = await appContext.App();
                        var version = await app.Version(AppVersionKey.Current);
                        value = version.Key().DisplayText;
                    }
                }
                else
                {
                    value = options.CacheBust;
                }
            }
            return value;
        }

        public async Task<string> Query()
        {
            var value = await Value();
            return string.IsNullOrWhiteSpace(value) ? "" : $"cacheBust={value}";
        }

        public override string ToString() => $"{nameof(CacheBust)} {value}";
    }
}
