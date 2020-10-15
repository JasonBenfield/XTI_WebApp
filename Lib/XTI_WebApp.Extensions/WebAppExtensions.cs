using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using XTI_App;
using XTI_App.Api;
using XTI_App.EF;
using XTI_Core;
using XTI_Secrets;

namespace XTI_WebApp.Extensions
{
    public static class WebAppExtensions
    {
        public static bool IsDevOrTest(this IHostEnvironment env) => env != null && (env.IsDevelopment() || env.IsEnvironment("Test"));

        public static void AddWebAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Views/Exports/Hub/{1}/{0}" + RazorViewEngine.ViewExtension);
            });
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.Configure<AppOptions>(configuration.GetSection(AppOptions.App));
            services.Configure<WebAppOptions>(configuration.GetSection(WebAppOptions.WebApp));
            services.Configure<DbOptions>(configuration.GetSection(DbOptions.DB));
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Jwt));
            services.Configure<SecretOptions>(configuration.GetSection(SecretOptions.Secret));
            var secretOptions = configuration.GetSection(SecretOptions.Secret).Get<SecretOptions>();
            services
                .AddDataProtection
                (
                    options => options.ApplicationDiscriminator = secretOptions.ApplicationName
                )
                .PersistKeysToFileSystem(new DirectoryInfo(secretOptions.KeyDirectoryPath))
                .SetApplicationName(secretOptions.ApplicationName);
            services.AddAppDbContext();
            services.AddScoped<CacheBust>();
            services.AddScoped<IPageContext, PageContext>();
            services.AddSingleton<Clock, UtcClock>();
            services.AddScoped<AppFactory, EfAppFactory>();
            AddXtiContextServices(services);
        }

        public static void AddAppDbContext(this IServiceCollection services)
        {
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
        }

        public static void AddXtiContextServices(this IServiceCollection services)
        {
            services.AddScoped<IAnonClient>(sp =>
            {
                var dataProtector = sp.GetDataProtector(new[] { $"XTI_Apps_Anon" });
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                return new AnonClient(dataProtector, httpContextAccessor);
            });
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var request = httpContextAccessor.HttpContext.Request;
                return XtiPath.Parse($"{request.PathBase}{request.Path}");
            });
            services.AddScoped<IAppContext>(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var cache = sp.GetService<IMemoryCache>();
                var appContext = sp.GetService<WebAppContext>();
                return new CachedAppContext(httpContextAccessor, cache, appContext);
            });
            services.AddScoped<IAppApiUser, XtiAppApiUser>();
            services.AddScoped<WebAppContext>();
            services.AddScoped<IUserContext>(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var cache = sp.GetService<IMemoryCache>();
                var sessionContext = sp.GetService<WebUserContext>();
                return new CachedUserContext(httpContextAccessor, cache, sessionContext);
            });
            services.AddScoped<WebUserContext>();
            services.AddScoped<ISessionContext, WebSessionContext>();
        }

    }
}
