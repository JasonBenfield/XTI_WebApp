using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using XTI_App;
using XTI_App.Api;
using XTI_App.Hosting;
using XTI_Core;
using XTI_TempLog;

namespace XTI_WebApp.Scheduled
{
    public sealed class WebActionRunnerFactory : IActionRunnerFactory
    {
        private readonly IServiceProvider services;

        public WebActionRunnerFactory(IServiceProvider services)
        {
            this.services = services;
        }

        public AppApi CreateAppApi()
        {
            var apiFactory = services.GetService<AppApiFactory>();
            return apiFactory.CreateForSuperUser();
        }

        public TempLogSession CreateTempLogSession()
        {
            var tempLog = services.GetService<TempLog>();
            var appEnvContext = services.GetService<ScheduledAppEnvironmentContext>();
            var clock = services.GetService<Clock>();
            var cache = services.GetService<IMemoryCache>();
            if (!cache.TryGetValue<CurrentSession>("scheduled_currentSession", out var currentSession))
            {
                currentSession = new CurrentSession();
                cache.Set("scheduled_currentSession", currentSession);
            }
            return new TempLogSession(tempLog, appEnvContext, currentSession, clock);
        }

        public XtiPath CreateXtiPath()
        {
            var appKey = services.GetService<AppKey>();
            return new XtiPath(appKey.Name);
        }
    }
}
