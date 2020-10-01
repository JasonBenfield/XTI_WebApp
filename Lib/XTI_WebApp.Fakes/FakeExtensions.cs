using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using XTI_App;
using XTI_App.EF;
using XTI_App.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using XTI_WebApp.Extensions;

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
            services
                .AddDataProtection(options =>
                {
                    options.ApplicationDiscriminator = "XTI_WEB_APP";
                })
                .SetApplicationName("XTI_WEB_APP");
            services.AddHttpContextAccessor();
            services.AddSingleton<Clock, FakeClock>();
            services.AddSingleton(sp => (FakeClock)sp.GetService<Clock>());
            services.AddSingleton<AppFactory, EfAppFactory>();
            services.AddScoped<ISessionContext, FakeSessionContext>();
            services.AddScoped<IAppContext, FakeAppContext>();
            services.AddSingleton<IAppApiUser, XtiAppApiUser>();
            services.AddSingleton<IHostEnvironment, FakeHostEnvironment>();
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var request = httpContextAccessor.HttpContext?.Request;
                return XtiPath.Parse($"{request?.PathBase}{request?.Path}");
            });
            services.AddScoped<CacheBust>();
            services.AddScoped<PageContext>();
            services.AddScoped<IHashedPasswordFactory, FakeHashedPasswordFactory>();
        }
    }
}
