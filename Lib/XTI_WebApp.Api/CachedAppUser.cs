using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XTI_App.Abstractions;
using XTI_App.Api;

namespace XTI_WebApp.Api
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

        public EntityID ID { get; }

        public AppUserName UserName() => userName;

        private IEnumerable<IAppRole> cachedUserRoles;

        public async Task<IEnumerable<IAppRole>> Roles(IApp app)
        {
            if (cachedUserRoles == null)
            {
                var userContext = httpContextAccessor.HttpContext.RequestServices.GetService<IUserContext>();
                var user = await userContext.User(ID.Value);
                var userRoles = await user.Roles(app);
                cachedUserRoles = userRoles
                    .Select(ur => new CachedAppRole(ur))
                    .ToArray();
            }
            return cachedUserRoles;
        }

        private readonly ConcurrentDictionary<int, bool> isModCategoryAdminLookup
            = new ConcurrentDictionary<int, bool>();

        public async Task<bool> IsModCategoryAdmin(IModifierCategory modCategory)
        {
            if (!isModCategoryAdminLookup.TryGetValue(modCategory.ID.Value, out var cachedIsAdmin))
            {
                var userContext = httpContextAccessor.HttpContext.RequestServices.GetService<IUserContext>();
                var user = await userContext.User(ID.Value);
                cachedIsAdmin = await user.IsModCategoryAdmin(modCategory);
                isModCategoryAdminLookup.AddOrUpdate(modCategory.ID.Value, cachedIsAdmin, (key, val) => cachedIsAdmin);
            }
            return cachedIsAdmin;
        }

        private readonly ConcurrentDictionary<string, bool> hasModifierLookup = new ConcurrentDictionary<string, bool>();

        public async Task<bool> HasModifier(ModifierKey modKey)
        {
            if (!hasModifierLookup.TryGetValue(modKey.Value, out var cachedHasModifier))
            {
                var userContext = httpContextAccessor.HttpContext.RequestServices.GetService<IUserContext>();
                var user = await userContext.User(ID.Value);
                cachedHasModifier = await user.HasModifier(modKey);
                hasModifierLookup.AddOrUpdate(modKey.Value, cachedHasModifier, (key, val) => cachedHasModifier);
            }
            return cachedHasModifier;
        }
    }
}
