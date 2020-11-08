using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using XTI_App;
using XTI_App.Api;
using XTI_App.DB;
using XTI_App.EF;
using XTI_App.Fakes;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_WebApp.Api;

namespace XTI_WebApp.Fakes
{
    public static class FakeExtensions
    {
        public static void AddFakesForXtiWebApp(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .EnableSensitiveDataLogging();
            });
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddDataProtection();
            services.AddSingleton<FakeClock>();
            services.AddSingleton<Clock, FakeClock>(sp => sp.GetService<FakeClock>());
            services.AddSingleton<AppFactory, EfAppFactory>();
            services.AddSingleton<IAnonClient, FakeAnonClient>();
            services.AddScoped<IAppApiUser, XtiAppApiUser>();
            services.AddSingleton<FakeWebHostEnvironment>();
            services.AddSingleton<IHostEnvironment, FakeWebHostEnvironment>();
            services.AddSingleton<IWebHostEnvironment, FakeWebHostEnvironment>();
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var request = httpContextAccessor.HttpContext?.Request;
                return XtiPath.Parse($"{request?.PathBase}{request?.Path}");
            });
            services.AddScoped<CacheBust>();
            services.AddScoped<IPageContext, PageContext>();
            services.AddScoped<IHashedPasswordFactory, FakeHashedPasswordFactory>();
        }

        public static void AddFakeXtiContexts(this IServiceCollection services)
        {
            services.AddScoped<IAppContext, DefaultAppContext>();
            services.AddScoped<IUserContext, FakeUserContext>();
            services.AddScoped<ISessionContext, WebSessionContext>();
        }

        public static void AddXtiContextServices(this IServiceCollection services)
        {
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
            services.AddScoped<ISessionContext, WebSessionContext>();
        }
    }
}
