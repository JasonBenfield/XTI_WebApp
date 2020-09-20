using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using XTI_App;
using XTI_App.EF;
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
            services
                .AddDataProtection(options =>
                {
                    options.ApplicationDiscriminator = "XTI_WEB_APP";
                })
                .SetApplicationName("XTI_WEB_APP");
            services.AddHttpContextAccessor();
            services.AddSingleton<Clock, FakeClock>();
            services.AddSingleton<AppFactory, EfAppFactory>();
        }

        public static void AddFakeAppApi(this IServiceCollection services)
        {
            services.AddSingleton<AppApi, FakeAppApi>();
        }
    }
}
