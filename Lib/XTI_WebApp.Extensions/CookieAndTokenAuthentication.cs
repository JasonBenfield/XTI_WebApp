using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using XTI_App;
using XTI_App.Abstractions;
using XTI_Core;

namespace XTI_WebApp.Extensions
{
    public static class CookieAndTokenAuthentication
    {
        public static void ConfigureXtiCookieAndTokenAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(4);
                    options.Cookie.Path = "/";
                    options.Cookie.Domain = "";
                    options.TicketDataFormat = createAuthTicketFormat(options.DataProtectionProvider, config);
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = x =>
                        {
                            if (x.Request.IsApiRequest())
                            {
                                if (x.HttpContext?.User?.Identity.IsAuthenticated ?? false)
                                {
                                    x.Response.StatusCode = StatusCodes.Status403Forbidden;
                                }
                                else
                                {
                                    x.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                }
                            }
                            else
                            {
                                redirectToLogin(x.HttpContext.RequestServices, x);
                            }
                            return Task.CompletedTask;
                        }
                    };
                    options.LogoutPath = "/Authenticator/Current/Auth/Logout";
                    options.AccessDeniedPath = options.LoginPath;
                    options.ReturnUrlParameter = "returnUrl";
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = createTokenValidationParameters(config);
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = c =>
                        {
                            return Task.CompletedTask;
                        },
                        OnChallenge = c =>
                        {
                            return Task.CompletedTask;
                        }
                    };
                });
            services.addAuthorization();
        }

        private static TokenValidationParameters createTokenValidationParameters(IConfiguration config)
        {
            var jwtOptions = config.GetSection(JwtOptions.Jwt).Get<JwtOptions>();
            var key = Encoding.ASCII.GetBytes(jwtOptions.Secret);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            return tokenValidationParameters;
        }

        private static JwtAuthTicketFormat createAuthTicketFormat(IDataProtectionProvider dataProtectionProvider, IConfiguration config)
        {
            var jwtOptions = config.GetSection(JwtOptions.Jwt).Get<JwtOptions>();
            var key = Encoding.ASCII.GetBytes(jwtOptions.Secret);
            var dataSerializer = new TicketSerializer();
            if (dataProtectionProvider == null)
            {
                var keyDirPath = new AppDataFolder()
                    .WithSubFolder("Keys")
                    .Path();
                dataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(keyDirPath));
            }
            var dataProtector = dataProtectionProvider.CreateProtector(new[] { "XTI_Apps_Auth1" });
            var authTicketFormat = new JwtAuthTicketFormat
            (
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                },
                dataSerializer,
                dataProtector
            );
            return authTicketFormat;
        }

        private static void redirectToLogin(IServiceProvider sp, RedirectContext<CookieAuthenticationOptions> x)
        {
            var options = sp.GetService<IOptions<AppOptions>>().Value;
            string startUrl;
            string returnUrl;
            string loginUrl;
            var isAuthenticator = x.Request.PathBase.ToString().StartsWith("/Authenticator/", StringComparison.OrdinalIgnoreCase);
            if (isAuthenticator || $"{x.Request.Scheme}://{x.Request.Host.Value}".Equals(options.BaseUrl, StringComparison.OrdinalIgnoreCase))
            {
                loginUrl = "/Authenticator/Current";
                startUrl = $"{x.Request.PathBase.Value}/User";
            }
            else
            {
                loginUrl = $"{options.BaseUrl}/Authenticator/Current";
                startUrl = $"{x.Request.Scheme}://{x.Request.Host.Value}{x.Request.PathBase.Value}/User";
            }
            if (x.Request.Path.HasValue)
            {
                returnUrl = $"{x.Request.Path.Value}";
            }
            else
            {
                returnUrl = $"/";
            }
            if (x.Request.QueryString.HasValue)
            {
                returnUrl += $"{x.Request.QueryString.Value}";
            }
            startUrl = HttpUtility.UrlEncode(startUrl);
            returnUrl = HttpUtility.UrlEncode(returnUrl);
            loginUrl = $"{loginUrl}/Auth?startUrl={startUrl}&returnUrl={returnUrl}";
            x.Response.Redirect(loginUrl);
        }

        private static bool IsApiRequest(this HttpRequest request)
        {
            return request != null && request.Method == "POST" && request.ContentType == "application/json";
        }

        private static void addAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder
                (
                    JwtBearerDefaults.AuthenticationScheme,
                    CookieAuthenticationDefaults.AuthenticationScheme
                )
                .RequireAuthenticatedUser()
                .Build();
            });
        }
    }
}
