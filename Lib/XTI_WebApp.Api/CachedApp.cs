using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class CachedApp : IApp
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CachedApp(IHttpContextAccessor httpContextAccessor, IApp app)
        {
            this.httpContextAccessor = httpContextAccessor;
            ID = app.ID;
            Title = app.Title;
        }

        public EntityID ID { get; }
        public string Title { get; }

        public Task<IAppVersion> CurrentVersion() => Version(AppVersionKey.Current);

        private readonly Dictionary<string, IAppVersion> cachedVersionLookup = new Dictionary<string, IAppVersion>();

        public async Task<IAppVersion> Version(AppVersionKey versionKey)
        {
            if (!cachedVersionLookup.TryGetValue(versionKey.Value, out var cachedVersion))
            {
                var app = await appFromContext();
                var version = await app.Version(versionKey);
                cachedVersion = new CachedAppVersion(version);
                cachedVersionLookup.Add(versionKey.Value, cachedVersion);
            }
            return cachedVersion;
        }

        private IEnumerable<IAppRole> cachedAppRoles;

        public async Task<IEnumerable<IAppRole>> Roles()
        {
            if (cachedAppRoles == null)
            {
                var app = await appFromContext();
                var appRoles = await app.Roles();
                cachedAppRoles = appRoles.Select(r => new CachedAppRole(r)).ToArray();
            }
            return cachedAppRoles;
        }

        private Task<IApp> appFromContext()
        {
            var appContext = httpContextAccessor.HttpContext.RequestServices.GetService<DefaultAppContext>();
            return appContext.App();
        }

        private readonly Dictionary<string, IResourceGroup> resourceGroupLookup = new Dictionary<string, IResourceGroup>();

        public async Task<IResourceGroup> ResourceGroup(ResourceGroupName name)
        {
            if (!resourceGroupLookup.TryGetValue(name.Value, out var cachedResourceGroup))
            {
                var app = await appFromContext();
                var group = await app.ResourceGroup(name);
                cachedResourceGroup = new CachedResourceGroup(httpContextAccessor, group);
                resourceGroupLookup.Add(name.Value, cachedResourceGroup);
            }
            return cachedResourceGroup;
        }
    }
}
