using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using XTI_App;
using XTI_App.Api;
using XTI_App.EF;
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
            services.AddDataProtection();
            services.AddSingleton<Clock, FakeClock>();
            services.AddSingleton(sp => (FakeClock)sp.GetService<Clock>());
            services.AddSingleton<AppFactory, EfAppFactory>();
            services.AddScoped<IUserContext, FakeUserContext>();
            services.AddScoped<IAppContext, FakeAppContext>();
            services.AddScoped<IAppApiUser, XtiAppApiUser>();
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
