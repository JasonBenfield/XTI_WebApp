using MainDB.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XTI_App;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_App.EfApi;
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
            services.AddMainDbContextForInMemory();
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
            services.AddTransient<IAppApiUser, XtiAppApiUser>();
            services.AddTransient(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var request = httpContextAccessor.HttpContext?.Request;
                var pathString = $"{request?.PathBase}{request?.Path}";
                if (string.IsNullOrWhiteSpace(pathString))
                {
                    var appKey = sp.GetService<AppKey>();
                    pathString = $"{appKey.Name.DisplayText}/{AppVersionKey.Current.DisplayText}".Replace(" ", "");
                }
                return XtiPath.Parse(pathString);
            });
            services.AddScoped(sp => sp.GetService<XtiPath>().Version);
            services.AddScoped<CacheBust>();
            services.AddScoped<IPageContext, PageContext>();
            services.AddScoped<IHashedPasswordFactory, FakeHashedPasswordFactory>();
            services.AddScoped<ISourceAppContext, DefaultAppContext>();
            services.AddScoped<IAppContext>(sp => sp.GetService<ISourceAppContext>());
            services.AddScoped<IUserContext, FakeUserContext>();
            services.AddScoped<CachedUserContext>();
            services.AddScoped<IAppEnvironmentContext, WebAppEnvironmentContext>();
            services.AddScoped<CurrentSession>();
            services.AddScoped(sp =>
            {
                var factory = sp.GetService<AppApiFactory>();
                var user = sp.GetService<IAppApiUser>();
                return factory.Create(user);
            });
            services.AddFakeTempLogServices();
        }
    }
}
