using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Extensions
{
    public sealed class WebSessionContext : ISessionContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AnonClient anonClient;
        private readonly AppFactory appFactory;
        private readonly Clock clock;

        public WebSessionContext(IHttpContextAccessor httpContextAccessor, AnonClient anonClient, AppFactory appFactory, Clock clock)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.anonClient = anonClient;
            this.appFactory = appFactory;
            this.clock = clock;
        }

        public async Task<IAppSession> Session()
        {
            IAppSession session;
            var httpUser = httpContextAccessor.HttpContext?.User;
            if (httpUser?.Identity.IsAuthenticated == true)
            {
                session = await authenticatedSession(httpUser, appFactory);
            }
            else
            {
                session = await anonSession(httpContextAccessor.HttpContext, clock, appFactory);
            }
            return session;
        }

        private static Task<AppSession> authenticatedSession(ClaimsPrincipal httpUser, AppFactory appFactory)
        {
            var sessionRepo = appFactory.SessionRepository();
            var sessionIDClaim = httpUser.Claims.First(c => c.Type == "SessionID");
            var sessionID = int.Parse(sessionIDClaim.Value);
            return sessionRepo.Session(sessionID);
        }

        private async Task<AppSession> anonSession(HttpContext context, Clock clock, AppFactory appFactory)
        {
            anonClient.Load();
            var sessionRepo = appFactory.SessionRepository();
            var session = await sessionRepo.Session(anonClient.SessionID);
            if (!session.HasStarted() || session.HasEnded())
            {
                var userRepo = appFactory.UserRepository();
                var anonUser = await userRepo.User(AppUserName.Anon);
                var requesterKey = anonClient.RequesterKey;
                if (string.IsNullOrWhiteSpace(requesterKey))
                {
                    requesterKey = Guid.NewGuid().ToString("N");
                }
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var remoteAddress = context.Connection.RemoteIpAddress?.ToString();
                session = await sessionRepo.Create(anonUser, clock.Now(), requesterKey, userAgent, remoteAddress);
                anonClient.Persist(session.ID, requesterKey);
            }
            return session;
        }

        public async Task<IAppUser> User()
        {
            AppUser user;
            var httpUser = httpContextAccessor.HttpContext?.User;
            if (httpUser?.Identity.IsAuthenticated == true)
            {
                var userIDClaim = httpUser.Claims.First(c => c.Type == "UserID");
                var userID = int.Parse(userIDClaim.Value);
                user = await appFactory.UserRepository().User(userID);
            }
            else
            {
                user = await appFactory.UserRepository().User(AppUserName.Anon);
            }
            return user;
        }
    }
}
