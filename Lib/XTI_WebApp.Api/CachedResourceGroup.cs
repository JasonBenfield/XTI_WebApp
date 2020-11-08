using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class CachedResourceGroup : IResourceGroup
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ResourceGroupName name;

        public CachedResourceGroup(IHttpContextAccessor httpContextAccessor, IResourceGroup group)
        {
            this.httpContextAccessor = httpContextAccessor;
            ID = group.ID;
            name = group.Name();
        }

        public EntityID ID { get; }
        public ResourceGroupName Name() => name;

        public async Task<IModifierCategory> ModCategory()
        {
            var cache = httpContextAccessor.HttpContext.RequestServices.GetService<IMemoryCache>();
            var key = $"group_{ID.Value}_modCategory";
            var cachedModCategory = cache.Get<CachedModifierCategory>(key);
            if (cachedModCategory == null)
            {
                var appContext = httpContextAccessor.HttpContext.RequestServices.GetService<DefaultAppContext>();
                var app = await appContext.App();
                var resourceGroup = await app.ResourceGroup(name);
                var modCategory = await resourceGroup.ModCategory();
                cachedModCategory = new CachedModifierCategory(modCategory);
                cache.Set(key, cachedModCategory);
            }
            return cachedModCategory;
        }

        public async Task<IResource> Resource(ResourceName name)
        {
            var cache = httpContextAccessor.HttpContext.RequestServices.GetService<IMemoryCache>();
            var key = $"group_{ID.Value}_modCategory";
            var cachedResource = cache.Get<CachedResource>(key);
            if (cachedResource == null)
            {
                var appContext = httpContextAccessor.HttpContext.RequestServices.GetService<DefaultAppContext>();
                var app = await appContext.App();
                var resourceGroup = await app.ResourceGroup(Name());
                var resource = await resourceGroup.Resource(name);
                cachedResource = new CachedResource(resource);
                cache.Set(key, cachedResource);
            }
            return cachedResource;
        }
    }
}
