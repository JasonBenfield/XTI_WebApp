using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XTI_App;

namespace XTI_WebApp.Extensions
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

        public int ID { get; }
        public string Title { get; }

        public async Task<IAppVersion> CurrentVersion()
        {
            var currentVersion = await fetch("current_version", async (app) =>
            {
                var version = await app.CurrentVersion();
                return new CachedAppVersion(version);
            });
            return currentVersion;
        }

        public async Task<IAppVersion> Version(int id)
        {
            var requestedVersion = await fetch($"version_{id}", async (app) =>
            {
                var version = await app.Version(id);
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
            var appContext = httpContextAccessor.HttpContext.RequestServices.GetService<WebAppContext>();
            return appContext.App();
        }
    }
}
