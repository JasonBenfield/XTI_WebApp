using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using XTI_App.Abstractions;
using XTI_App.EfApi;
using Microsoft.Extensions.DependencyInjection;

namespace XTI_WebApp.Api
{
    public sealed class CachedAppVersion : IAppVersion
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AppVersionKey key;

        public CachedAppVersion(IHttpContextAccessor httpContextAccessor, IAppVersion appVersion)
        {
            this.httpContextAccessor = httpContextAccessor;
            ID = appVersion.ID;
            key = appVersion.Key();
        }

        public EntityID ID { get; }

        public AppVersionKey Key() => key;

        private readonly ConcurrentDictionary<string, CachedResourceGroup> resourceGroupLookup
            = new ConcurrentDictionary<string, CachedResourceGroup>();

        public async Task<IResourceGroup> ResourceGroup(ResourceGroupName name)
        {
            if (!resourceGroupLookup.TryGetValue(name.Value, out var cachedResourceGroup))
            {
                var app = await appFromContext();
                var version = await app.Version(key);
                var group = await version.ResourceGroup(name);
                cachedResourceGroup = new CachedResourceGroup(httpContextAccessor, group);
                resourceGroupLookup.AddOrUpdate(name.Value, cachedResourceGroup, (key, rg) => cachedResourceGroup);
            }
            return cachedResourceGroup;
        }

        private Task<IApp> appFromContext()
        {
            var appContext = httpContextAccessor.HttpContext.RequestServices.GetService<DefaultAppContext>();
            return appContext.App();
        }
    }
}
