using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_TempLog;

namespace XTI_WebApp
{
    public sealed class WebAppEnvironmentContext : IAppEnvironmentContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAnonClient anonClient;
        private readonly IUserContext userContext;

        private AppEnvironment value;

        public WebAppEnvironmentContext(IHttpContextAccessor httpContextAccessor, IAnonClient anonClient, IUserContext userContext)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.anonClient = anonClient;
            this.userContext = userContext;
        }

        public async Task<AppEnvironment> Value()
        {
            if (value == null)
            {
                anonClient.Load();
                var user = await userContext.User();
                var requesterKey = anonClient.RequesterKey;
                if (string.IsNullOrWhiteSpace(requesterKey))
                {
                    requesterKey = Guid.NewGuid().ToString("N");
                }
                var userAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
                var remoteAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                value = new AppEnvironment
                (
                    user.UserName().Value,
                    requesterKey,
                    remoteAddress,
                    userAgent,
                    AppType.Values.WebApp.DisplayText
                );
            }
            return value;
        }
    }
}
