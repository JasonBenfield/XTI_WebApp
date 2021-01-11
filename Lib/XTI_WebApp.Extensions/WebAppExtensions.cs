using MainDB.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_Secrets.Extensions;
using XTI_TempLog;
using XTI_TempLog.Extensions;
using XTI_WebApp.Api;

namespace XTI_WebApp.Extensions
{
    public static class WebAppExtensions
    {
        public static bool IsDevOrTest(this IHostEnvironment env) => env != null && (env.IsDevelopment() || env.IsEnvironment("Test"));

        public static void AddWebAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Views/Exports/Shared/{1}/{0}" + RazorViewEngine.ViewExtension);
            });
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.Configure<AppOptions>(configuration.GetSection(AppOptions.App));
            services.Configure<WebAppOptions>(configuration.GetSection(WebAppOptions.WebApp));
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Jwt));
            services.AddXtiDataProtection();
            services.AddAppDbContextForSqlServer(configuration);
            services.AddScoped<CacheBust>();
            services.AddScoped<IPageContext, PageContext>();
            services.AddSingleton<Clock, UtcClock>();
            services.AddScoped<AppFactory>();
            services.AddSingleton(sp =>
            {
                var hostEnv = sp.GetService<IHostEnvironment>();
                var appKey = sp.GetService<AppKey>();
                return new AppDataFolder()
                    .WithHostEnvironment(hostEnv)
                    .WithSubFolder("WebApps")
                    .WithSubFolder(appKey.Name.DisplayText);
            });
            services.AddScoped<CurrentSession>();
            services.AddScoped(sp =>
            {
                var factory = sp.GetService<AppApiFactory>();
                var user = sp.GetService<IAppApiUser>();
                return factory.Create(user);
            });
            services.AddTempLogServices();
            AddXtiContextServices(services);
        }

        private static void AddXtiContextServices(this IServiceCollection services)
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
                var appContext = sp.GetService<DefaultAppContext>();
                return new CachedAppContext(httpContextAccessor, cache, appContext);
            });
            services.AddScoped<IAppApiUser, XtiAppApiUser>();
            services.AddScoped<DefaultAppContext>();
            services.AddScoped<IUserContext>(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var cache = sp.GetService<IMemoryCache>();
                var sessionContext = sp.GetService<WebUserContext>();
                return new CachedUserContext(httpContextAccessor, cache, sessionContext);
            });
            services.AddScoped<WebUserContext>();
            services.AddScoped<IAppEnvironmentContext, WebAppEnvironmentContext>();
        }

        public static void SetDefaultJsonOptions(this JsonOptions options)
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            options.JsonSerializerOptions.IgnoreNullValues = true;
        }

        public static void SetDefaultMvcOptions(this MvcOptions options)
        {
            options.CacheProfiles.Add("Default", new CacheProfile
            {
                Duration = 2592000,
                Location = ResponseCacheLocation.Any,
                NoStore = false
            });
            options.ModelBinderProviders.Insert(0, new FormModelBinderProvider());
        }

        public static void UseDefaultResponseCaching(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(30)
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Accept-Encoding" };
                await next();
            });
        }
    }
}
