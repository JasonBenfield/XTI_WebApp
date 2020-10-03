using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Extensions
{
    public sealed class SessionLog
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AppFactory appFactory;
        private readonly IUserContext sessionContext;
        private readonly AnonClient anonClient;
        private readonly Clock clock;

        public SessionLog(IHttpContextAccessor httpContextAccessor, AppFactory appFactory, IUserContext sessionContext, AnonClient anonClient, Clock clock)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.appFactory = appFactory;
            this.sessionContext = sessionContext;
            this.anonClient = anonClient;
            this.clock = clock;
        }

        public async Task<AppSession> Session()
        {
            AppSession session;
            var httpUser = httpContextAccessor.HttpContext?.User;
            if (httpUser?.Identity.IsAuthenticated == true)
            {
                session = await authenticatedSession();
            }
            else
            {
                session = await anonSession();
            }
            return session;
        }

        private Task<AppSession> authenticatedSession()
        {
            var sessionRepo = appFactory.Sessions();
            var sessionID = new XtiClaims(httpContextAccessor).SessionID();
            return sessionRepo.Session(sessionID);
        }

        private async Task<AppSession> anonSession()
        {
            anonClient.Load();
            var sessionRepo = appFactory.Sessions();
            var session = await sessionRepo.Session(anonClient.SessionID);
            if (!session.HasStarted() || session.HasEnded())
            {
                var anonUser = await sessionContext.User();
                var requesterKey = anonClient.RequesterKey;
                if (string.IsNullOrWhiteSpace(requesterKey))
                {
                    requesterKey = Guid.NewGuid().ToString("N");
                }
                var userAgent = httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
                var remoteAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                session = await sessionRepo.Create(anonUser, clock.Now(), requesterKey, userAgent, remoteAddress);
                anonClient.Persist(session.ID, requesterKey);
            }
            return session;
        }

    }
}
