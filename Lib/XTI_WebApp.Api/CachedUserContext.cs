using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using XTI_App.Abstractions;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class CachedUserContext : IUserContext
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
            cacheUser($"user_{user.ID.Value}", new CachedAppUser(httpContextAccessor, user));
        }

        public Task<IAppUser> User()
        {
            var claims = new XtiClaims(httpContextAccessor);
            var userID = claims.UserID();
            return User(userID);
        }

        public async Task<IAppUser> User(int userID)
        {
            var userKey = $"user_{userID}";
            if (!cache.TryGetValue(userKey, out CachedAppUser cachedUser))
            {
                var user = await source.User(userID);
                cachedUser = new CachedAppUser(httpContextAccessor, user);
                cacheUser(userKey, cachedUser);
            }
            return cachedUser;
        }

        private void cacheUser(string key, CachedAppUser user)
        {
            cache.Set
            (
                key,
                user,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = new TimeSpan(1, 0, 0)
                }
            );
        }
    }
}
