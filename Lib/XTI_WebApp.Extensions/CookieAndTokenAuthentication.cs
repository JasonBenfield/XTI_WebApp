using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace XTI_WebApp.Extensions
{
    public static class CookieAndTokenAuthentication
    {
        public static void ConfigureXtiCookieAndTokenAuthentication(this IServiceCollection services)
        {
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(4);
                    var sp = services.BuildServiceProvider();
                    var jwtOptions = sp.GetService<IOptions<JwtOptions>>().Value;
                    var key = Encoding.ASCII.GetBytes(jwtOptions.Secret);
                    options.TicketDataFormat = new JwtAuthTicketFormat
                    (
                        new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false
                        },
                        sp.GetService<IDataSerializer<AuthenticationTicket>>(),
                        sp.GetDataProtector(new[] { $"XTI_Apps_Auth1" })
                    );
                    options.Cookie.Path = "/";
                    options.Cookie.Domain = "";
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = x =>
                        {
                            return Task.CompletedTask;
                        },
                        OnSigningIn = x =>
                        {
                            return Task.CompletedTask;
                        },
                        OnRedirectToLogin = x =>
                        {
                            if (x.Request.IsApiRequest())
                            {
                                if ((x.HttpContext.User?.Identity.IsAuthenticated ?? false))
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
                                redirectToLogin(sp, x);
                            }
                            return Task.CompletedTask;
                        }
                    };
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = options.LoginPath;
                    options.ReturnUrlParameter = "returnUrl";
                })
                .AddJwtBearer(x =>
                {
                    var sp = services.BuildServiceProvider();
                    var jwtOptions = sp.GetService<IOptions<JwtOptions>>().Value;
                    var key = Encoding.ASCII.GetBytes(jwtOptions.Secret);
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
            services.addAuthorization();
        }

        private static void redirectToLogin(ServiceProvider sp, RedirectContext<CookieAuthenticationOptions> x)
        {
            var options = sp.GetService<IOptions<WebAppOptions>>().Value;
            string startUrl;
            string returnUrl;
            string loginUrl;
            var isHub = x.Request.PathBase.Value.Equals("/Hub", StringComparison.OrdinalIgnoreCase);
            if
            (
                !isHub &&
                !$"{x.Request.Scheme}://{x.Request.Host.Value}".Equals(options.BaseUrl, StringComparison.OrdinalIgnoreCase)
            )
            {
                startUrl = $"{x.Request.Scheme}://{x.Request.Host.Value}{x.Request.PathBase.Value}/User";
                loginUrl = $"{options.BaseUrl}/";
            }
            else
            {
                if (isHub)
                {
                    loginUrl = "/";
                }
                else
                {
                    loginUrl = "/Hub";
                }
                startUrl = $"{x.Request.PathBase.Value}/User";
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
            loginUrl = $"{loginUrl}Hub/Auth?startUrl={startUrl}&returnUrl={returnUrl}";
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
