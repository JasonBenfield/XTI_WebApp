using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Reflection;
using XTI_App;
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
            services.Configure<AppDbOptions>(configuration.GetSection(AppDbOptions.AppDb));
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Jwt));
            var webAppOptions = services.BuildServiceProvider().GetService<IOptions<WebAppOptions>>().Value;
            services
                .AddDataProtection
                (
                    options => options.ApplicationDiscriminator = appName
                )
                .PersistKeysToFileSystem(new DirectoryInfo(webAppOptions.KeyFolder))
                .SetApplicationName(appName);
            services.AddScoped(sp => createCacheBust(sp, assembly));
            services.AddScoped(createPageContext);
            services.AddDbContext<AppDbContext>(optionsAction: (sp, dbOptionsBuilder) =>
            {
                var appDbOptions = sp.GetService<IOptions<AppDbOptions>>().Value;
                dbOptionsBuilder.UseSqlServer(appDbOptions.ConnectionString);
                var hostingEnv = sp.GetService<IWebHostEnvironment>();
                if (hostingEnv.IsDevOrTest())
                {
                    dbOptionsBuilder.EnableSensitiveDataLogging();
                }
            });
            services.AddSingleton<Clock, UtcClock>();
            services.AddScoped<AppFactory, EfAppFactory>();
        }

        private static CacheBust createCacheBust(IServiceProvider sp, Assembly assembly)
        {
            string cacheBust;
            var options = sp.GetService<IOptions<WebAppOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.CacheBust))
            {
                var hostingEnv = sp.GetService<IWebHostEnvironment>();
                if (hostingEnv.IsDevOrTest())
                {
                    cacheBust = Guid.NewGuid().ToString("N");
                }
                else
                {
                    var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                    cacheBust = version;
                }
            }
            else
            {
                cacheBust = options.CacheBust;
            }
            return new CacheBust(cacheBust);
        }

        private static PageContext createPageContext(IServiceProvider sp)
        {
            var options = sp.GetService<IOptions<WebAppOptions>>().Value;
            var cacheBust = sp.GetService<CacheBust>();
            return new PageContext
            {
                BaseUrl = string.IsNullOrWhiteSpace(options.BaseUrl) ? "/" : options.BaseUrl,
                CacheBust = cacheBust.Value
            };
        }

    }
}
