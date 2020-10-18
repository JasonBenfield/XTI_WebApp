using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using XTI_App;
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
            cache.Set($"user_{user.ID}", new CachedAppUser(httpContextAccessor, user));
            source.RefreshUser(user);
        }

        public async Task<IAppUser> User(int userID)
        {
            string userKey;
            if (userID == 0)
            {
                userKey = "user_anon";
            }
            else
            {
                userKey = $"user_{userID}";
            }
            if (!cache.TryGetValue(userKey, out IAppUser cachedUser))
            {
                var user = await source.User();
                cachedUser = new CachedAppUser(httpContextAccessor, user);
                cache.Set(userKey, cachedUser);
            }
            return cachedUser;
        }

        public Task<IAppUser> User()
        {
            var claims = new XtiClaims(httpContextAccessor);
            return User(claims.UserID());
        }
    }
}
