using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class CachedAppContext : IAppContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMemoryCache cache;
        private readonly IAppContext source;

        public CachedAppContext(IHttpContextAccessor httpContextAccessor, IMemoryCache cache, IAppContext source)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.cache = cache;
            this.source = source;
        }

        public async Task<IApp> App()
        {
            if (!cache.TryGetValue("xti_app", out CachedApp cachedApp))
            {
                var app = await source.App();
                cachedApp = new CachedApp(httpContextAccessor, app);
                cache.Set("xti_app", cachedApp);
            }
            return cachedApp;
        }
    }
}
