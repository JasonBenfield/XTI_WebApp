using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;

namespace XTI_WebApp.Extensions
{
    public sealed class WebSessionContext : ISessionContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AppFactory appFactory;
        private readonly IAppContext appContext;
        private readonly IUserContext userContext;
        private readonly XtiPath xtiPath;
        private readonly IAnonClient anonClient;
        private readonly Clock clock;

        public WebSessionContext
        (
            IHttpContextAccessor httpContextAccessor,
            AppFactory appFactory,
            IAppContext appContext,
            IUserContext userContext,
            XtiPath xtiPath,
            IAnonClient anonClient,
            Clock clock
        )
        {
            this.httpContextAccessor = httpContextAccessor;
            this.appFactory = appFactory;
            this.appContext = appContext;
            this.xtiPath = xtiPath;
            this.userContext = userContext;
            this.anonClient = anonClient;
            this.clock = clock;
        }

        public AppSession CurrentSession { get; private set; }

        public AppRequest CurrentRequest { get; private set; }

        public async Task StartSession()
        {
            var user = await userContext.User();
            if (user.UserName().Equals(AppUserName.Anon))
            {
                CurrentSession = await anonSession(user);
            }
            else
            {
                CurrentSession = await authenticatedSession();
            }
        }

        private Task<AppSession> authenticatedSession()
        {
            var sessionRepo = appFactory.Sessions();
            var sessionID = new XtiClaims(httpContextAccessor).SessionID();
            return sessionRepo.Session(sessionID);
        }

        private async Task<AppSession> anonSession(IAppUser user)
        {
            anonClient.Load();
            var sessionRepo = appFactory.Sessions();
            var session = await sessionRepo.Session(anonClient.SessionID);
            if (!session.HasStarted() || session.HasEnded())
            {
                var requesterKey = anonClient.RequesterKey;
                if (string.IsNullOrWhiteSpace(requesterKey))
                {
                    requesterKey = Guid.NewGuid().ToString("N");
                }
                var userAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
                var remoteAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                session = await sessionRepo.Create(user, clock.Now(), requesterKey, userAgent, remoteAddress);
                anonClient.Persist(session.ID, requesterKey);
            }
            return session;
        }

        public async Task StartRequest()
        {
            var version = await retrieveVersion(appContext, xtiPath);
            var path = httpContextAccessor.HttpContext?.Request.Path;
            CurrentRequest = await CurrentSession.LogRequest(version, path, clock.Now());
        }

        private static async Task<IAppVersion> retrieveVersion(IAppContext appContext, XtiPath xtiPath)
        {
            var app = await appContext.App();
            IAppVersion version;
            if (xtiPath.IsCurrentVersion())
            {
                version = await app.CurrentVersion();
            }
            else
            {
                version = await app.Version(AppVersionKey.Parse(xtiPath.Version));
            }
            return version;
        }

    }
}
