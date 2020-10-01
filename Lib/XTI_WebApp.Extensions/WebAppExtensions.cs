using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Reflection;
using XTI_App;
using XTI_App.Api;
using XTI_App.EF;

namespace XTI_WebApp.Extensions
{
    public static class WebAppExtensions
    {
        public const string appName = "XTI_Web_Apps";

        public static bool IsDevOrTest(this IHostEnvironment env) => env != null && (env.IsDevelopment() || env.IsEnvironment("Test"));

        public static void AddXtiServices(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddHttpContextAccessor();
            services.Configure<WebAppOptions>(configuration.GetSection(WebAppOptions.WebApp));
            services.Configure<DbOptions>(configuration.GetSection(DbOptions.DB));
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Jwt));
            var webAppOptions = services.BuildServiceProvider().GetService<IOptions<WebAppOptions>>().Value;
            services
                .AddDataProtection
                (
                    options => options.ApplicationDiscriminator = appName
                )
                .PersistKeysToFileSystem(new DirectoryInfo(webAppOptions.KeyFolder))
                .SetApplicationName(appName);
            services.AddScoped<CacheBust>();
            services.AddScoped<PageContext>();
            services.AddDbContext<AppDbContext>(optionsAction: (sp, dbOptionsBuilder) =>
            {
                var appDbOptions = sp.GetService<IOptions<DbOptions>>().Value;
                var hostingEnv = sp.GetService<IHostEnvironment>();
                dbOptionsBuilder.UseSqlServer(new AppConnectionString(appDbOptions, hostingEnv.EnvironmentName).Value());
                if (hostingEnv.IsDevOrTest())
                {
                    dbOptionsBuilder.EnableSensitiveDataLogging();
                }
            });
            services.AddSingleton<Clock, UtcClock>();
            services.AddScoped<AppFactory, EfAppFactory>();
            services.AddScoped(sp =>
            {
                var dataProtector = sp.GetDataProtector(new[] { $"{appName}-Anon" });
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                return new AnonClient(dataProtector, httpContextAccessor);
            });
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var request = httpContextAccessor.HttpContext.Request;
                return XtiPath.Parse($"{request.PathBase}{request.Path}");
            });
            services.AddScoped<IAppContext, WebAppContext>();
            services.AddScoped<ISessionContext, WebSessionContext>();
        }
    }
}
