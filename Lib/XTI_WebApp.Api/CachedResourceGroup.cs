using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using XTI_App.Abstractions;
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

        private IModifierCategory cachedModCategory;

        public async Task<IModifierCategory> ModCategory()
        {
            if (cachedModCategory == null)
            {
                var version = await versionFromContext();
                var resourceGroup = await version.ResourceGroup(name);
                var modCategory = await resourceGroup.ModCategory();
                cachedModCategory = new CachedModifierCategory(modCategory);
            }
            return cachedModCategory;
        }

        private readonly ConcurrentDictionary<string, CachedResource> cachedResourceLookup
            = new ConcurrentDictionary<string, CachedResource>();

        public async Task<IResource> Resource(ResourceName name)
        {
            if (!cachedResourceLookup.TryGetValue(name.Value, out var cachedResource))
            {
                var version = await versionFromContext();
                var resourceGroup = await version.ResourceGroup(Name());
                var resource = await resourceGroup.Resource(name);
                cachedResource = new CachedResource(resource);
                cachedResourceLookup.AddOrUpdate(name.Value, cachedResource, (key, r) => cachedResource);
            }
            return cachedResource;
        }

        private async Task<IAppVersion> versionFromContext()
        {
            var appContext = httpContextAccessor.HttpContext.RequestServices.GetService<ISourceAppContext>();
            var version = await appContext.Version();
            return version;
        }

    }
}
