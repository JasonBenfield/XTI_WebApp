using MainDB.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XTI_App;
using XTI_App.Api;
using XTI_App.Fakes;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_TempLog;
using XTI_TempLog.Fakes;
using XTI_WebApp.Api;

namespace XTI_WebApp.Fakes
{
    public static class FakeExtensions
    {
        public static void AddFakesForXtiWebApp(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAppDbContextForInMemory();
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddDataProtection();
            services.Configure<AppOptions>(configuration.GetSection(AppOptions.App));
            services.Configure<WebAppOptions>(configuration.GetSection(WebAppOptions.WebApp));
            services.AddSingleton<FakeClock>();
            services.AddSingleton<Clock, FakeClock>(sp => sp.GetService<FakeClock>());
            services.AddScoped<AppFactory>();
            services.AddScoped<IAnonClient, FakeAnonClient>();
            services.AddScoped<IAppApiUser, XtiAppApiUser>();
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var request = httpContextAccessor.HttpContext?.Request;
                return XtiPath.Parse($"{request?.PathBase}{request?.Path}");
            });
            services.AddScoped<CacheBust>();
            services.AddScoped<IPageContext, PageContext>();
            services.AddScoped<IHashedPasswordFactory, FakeHashedPasswordFactory>();
            services.AddScoped<DefaultAppContext>();
            services.AddScoped<IAppContext>(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var cache = sp.GetService<IMemoryCache>();
                var appContext = sp.GetService<DefaultAppContext>();
                return new CachedAppContext(httpContextAccessor, cache, appContext);
            });
            services.AddScoped<WebUserContext>();
            services.AddScoped<IUserContext>(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var cache = sp.GetService<IMemoryCache>();
                var sessionContext = sp.GetService<WebUserContext>();
                return new CachedUserContext(httpContextAccessor, cache, sessionContext);
            });
            services.AddScoped<IAppEnvironmentContext, WebAppEnvironmentContext>();
            services.AddFakeTempLogServices();
        }
    }
}
