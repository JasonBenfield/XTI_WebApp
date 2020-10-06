using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XTI_App;

namespace XTI_WebApp.Extensions
{
    public sealed class CachedAppUser : IAppUser
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AppUserName userName;

        public CachedAppUser(IHttpContextAccessor httpContextAccessor, IAppUser source)
        {
            this.httpContextAccessor = httpContextAccessor;
            ID = source.ID;
            userName = source.UserName();
        }

        public int ID { get; }

        public AppUserName UserName() => userName;

        public async Task<IEnumerable<IAppUserRole>> RolesForApp(IApp app)
        {
            var cache = httpContextAccessor.HttpContext.RequestServices.GetService<IMemoryCache>();
            var key = $"user_{ID}_roles_app_{app.ID}";
            var cachedUserRoles = cache.Get<IEnumerable<CachedAppUserRole>>(key);
            if (cachedUserRoles == null)
            {
                var sessionContext = httpContextAccessor.HttpContext.RequestServices.GetService<WebUserContext>();
                var user = await sessionContext.User(ID);
                var userRoles = await user.RolesForApp(app);
                cachedUserRoles = userRoles
                    .Select(ur => new CachedAppUserRole(ur))
                    .ToArray();
                cache.Set(key, userRoles);
            }
            return cachedUserRoles;
        }
    }
}
