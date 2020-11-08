using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
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

        public async Task<IAppVersion> Version(AppVersionKey versionKey)
        {
            var requestedVersion = await fetch($"version_{versionKey.Value}", async (app) =>
            {
                var version = await app.Version(versionKey);
                return new CachedAppVersion(version);
            });
            return requestedVersion;
        }

        public async Task<IEnumerable<IAppRole>> Roles()
        {
            var appRoles = await fetch("app_roles", async (app) =>
            {
                var roles = await app.Roles();
                return roles.Select(r => new CachedAppRole(r)).ToArray();
            });
            return appRoles;
        }

        private async Task<T> fetch<T>(string key, Func<IApp, Task<T>> createValue)
        {
            var cache = httpContextAccessor.HttpContext.RequestServices.GetService<IMemoryCache>();
            if (!cache.TryGetValue(key, out T cachedValue))
            {
                var app = await appFromContext();
                cachedValue = await createValue(app);
                cache.Set(key, cachedValue);
            }
            return cachedValue;
        }

        private Task<IApp> appFromContext()
        {
            var appContext = httpContextAccessor.HttpContext.RequestServices.GetService<DefaultAppContext>();
            return appContext.App();
        }

        public async Task<IResourceGroup> ResourceGroup(ResourceGroupName name)
        {
            var requestedGroup = await fetch($"group_{name.Value}", async (app) =>
            {
                var group = await app.ResourceGroup(name);
                return new CachedResourceGroup(httpContextAccessor, group);
            });
            return requestedGroup;
        }
    }
}
