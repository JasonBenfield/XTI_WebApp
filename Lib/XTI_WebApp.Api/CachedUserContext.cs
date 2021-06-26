using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Abstractions;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public class CachedUserContext : IUserContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMemoryCache cache;
        private readonly IUserContext source;

        public CachedUserContext(IHttpContextAccessor httpContextAccessor, IMemoryCache cache, IUserContext source)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.cache = cache;
            this.source = source;
        }

        public void RefreshUser(IAppUser user)
        {
            cache.Set($"user_{user.ID.Value}", new CachedAppUser(httpContextAccessor, user));
            source.RefreshUser(user);
        }

        public async Task<IAppUser> User()
        {
            var claims = new XtiClaims(httpContextAccessor);
            var userID = claims.UserID();
            var userKey = $"user_{userID}";
            if (!cache.TryGetValue(userKey, out CachedAppUser cachedUser))
            {
                var user = await source.User();
                cachedUser = new CachedAppUser(httpContextAccessor, user);
                cache.Set(userKey, cachedUser);
            }
            return cachedUser;
        }

        public Task<AppUser> UncachedUser()
        {
            var claims = new XtiClaims(httpContextAccessor);
            var userID = claims.UserID();
            var factory = httpContextAccessor.HttpContext.RequestServices.GetService<AppFactory>();
            return factory.Users().User(userID);
        }
    }
}
