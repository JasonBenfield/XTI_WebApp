using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using XTI_App;
using XTI_App.Abstractions;
using XTI_WebApp.Extensions;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.AspTests
{
#pragma warning disable CS0162
    public class XtiAuthenticationTest
    {

        private static readonly string baseUrl = "https://webapps.xartogg.com";
        [Test]
        public async Task ShouldRedirectToLogin()
        {
            var pathBase = "/Authenticator/Current";
            var input = await setup(pathBase);
            input.Host.GetTestServer().BaseAddress = new Uri(baseUrl);
            var response = await input.GetAsync("/UserAdmin/Index");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Found));
            var startUrl = HttpUtility.UrlEncode("/Authenticator/Current/User");
            var returnUrl = HttpUtility.UrlEncode("/UserAdmin/Index");
            Assert.That
            (
                response.Headers.Location.ToString(),
                Is.EqualTo($"/Authenticator/Current/Auth?startUrl={startUrl}&returnUrl={returnUrl}")
            );
        }

        [Test]
        public async Task ShouldRedirectToLogin_WhenCalledFromADifferentApp()
        {
            var pathBase = "/Fake/Current";
            var input = await setup(pathBase);
            input.Host.GetTestServer().BaseAddress = new Uri(baseUrl);
            var response = await input.GetAsync("/UserAdmin/Index");
            var startUrl = HttpUtility.UrlEncode("/Fake/Current/User");
            var returnUrl = HttpUtility.UrlEncode("/UserAdmin/Index");
            Assert.That
            (
                response.Headers.Location.ToString(),
                Is.EqualTo($"/Authenticator/Current/Auth?startUrl={startUrl}&returnUrl={returnUrl}")
            );
        }

        [Test]
        public async Task ShouldIncludeQueryInRedirectUrl()
        {
            var pathBase = "/Fake/Current";
            var input = await setup(pathBase);
            input.Host.GetTestServer().BaseAddress = new Uri("https://webapps.xartogg.com");
            var response = await input.GetAsync("/UserAdmin/Index?test1=1&test2=2");
            var startUrl = HttpUtility.UrlEncode("/Fake/Current/User");
            var returnUrl = HttpUtility.UrlEncode("/UserAdmin/Index?test1=1&test2=2");
            Assert.That
            (
                response.Headers.Location.ToString(),
                Is.EqualTo($"/Authenticator/Current/Auth?startUrl={startUrl}&returnUrl={returnUrl}")
            );
        }

        [Test]
        public async Task ShouldRedirectToLoginWithAbsoluteUrl_WhenCalledFromADifferentAppWithADifferentDomain()
        {
            var pathBase = "/Fake/Current";
            var input = await setup(pathBase);
            input.Host.GetTestServer().BaseAddress = new Uri("https://webapps.xartogg.com:44303");
            var response = await input.GetAsync("/UserAdmin/Index");
            var startUrl = HttpUtility.UrlEncode("https://webapps.xartogg.com:44303/Fake/Current/User");
            var returnUrl = HttpUtility.UrlEncode("/UserAdmin/Index");
            Assert.That
            (
                response.Headers.Location.ToString(),
                Is.EqualTo($"https://webapps.xartogg.com/Authenticator/Current/Auth?startUrl={startUrl}&returnUrl={returnUrl}")
            );
        }

        [Test]
        public async Task ShouldRedirectToLogin_WhenCalledFromAuthenticatorWithADifferentDomain()
        {
            var pathBase = "/Authenticator/Current";
            var input = await setup(pathBase);
            input.Host.GetTestServer().BaseAddress = new Uri("https://webapps.xartogg.com:44303");
            var response = await input.GetAsync("/UserAdmin/Index");
            var startUrl = HttpUtility.UrlEncode("/Authenticator/Current/User");
            var returnUrl = HttpUtility.UrlEncode("/UserAdmin/Index");
            Assert.That
            (
                response.Headers.Location.ToString(),
                Is.EqualTo($"/Authenticator/Current/Auth?startUrl={startUrl}&returnUrl={returnUrl}")
            );
        }

        public sealed class MainApp
        {
            public MainApp()
            {
                Action = c => c.ChallengeAsync();
            }

            public Func<HttpContext, Task> Action { get; set; }

            public Task Execute(HttpContext c) => Action(c);
        }

        private async Task<TestInput> setup(string pathBase)
        {
            var hostBuilder = new HostBuilder();
            var host = await hostBuilder
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .UseUrls("https://webapps.xartogg.com/Authenticator/Current")
                        .ConfigureServices((context, services) =>
                        {
                            services.AddSingleton<TestAuthOptions>();
                            services.Configure<JwtOptions>(context.Configuration.GetSection(JwtOptions.Jwt));
                            services.Configure<AppOptions>(context.Configuration.GetSection(AppOptions.App));
                            services.AddDataProtection();
                            services.ConfigureXtiCookieAndTokenAuthentication(context.Configuration);
                            services.AddFakesForXtiWebApp(context.Configuration);
                            services.AddSingleton<MainApp>();
                        })
                        .Configure(app =>
                        {
                            app.UsePathBase(pathBase);
                            app.UseAuthentication();
                            app.UseAuthorization();
                            app.Run(c =>
                            {
                                c.Request.PathBase = pathBase;
                                var mainApp = c.RequestServices.GetService<MainApp>();
                                return mainApp.Execute(c);
                            });
                        });
                })
                .ConfigureAppConfiguration
                (
                    (hostingContext, config) =>
                    {
                        config.Sources.Clear();
                        config.AddInMemoryCollection(new[]
                        {
                            KeyValuePair.Create("Jwt:Secret", "Secret for token"),
                            KeyValuePair.Create("App:BaseUrl", baseUrl)
                        });
                    }
                )
                .StartAsync();
            var jwtOptions = host.Services.GetService<IOptions<JwtOptions>>().Value;
            var appOptions = host.Services.GetService<IOptions<AppOptions>>().Value;
            return new TestInput(host);
        }

        private sealed class TestInput
        {
            public TestInput(IHost host)
            {
                Host = host;
                Cookies = new CookieContainer();
            }
            public IHost Host { get; }
            public CookieContainer Cookies { get; }

            public async Task<HttpResponseMessage> GetAsync(string relativeUrl)
            {
                var testServer = Host.GetTestServer();
                var absoluteUrl = new Uri(testServer.BaseAddress, relativeUrl);
                var requestBuilder = testServer.CreateRequest(absoluteUrl.ToString());
                requestBuilder.AddHeader(HeaderNames.Authorization, "Test");
                requestBuilder.AddHeader(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.52 Safari/537.36 OPR/15.0.1147.100");
                AddCookies(requestBuilder, absoluteUrl);
                var response = await requestBuilder.GetAsync();
                UpdateCookies(response, absoluteUrl);
                return response;
            }

            private void AddCookies(RequestBuilder requestBuilder, Uri absoluteUrl)
            {
                var cookieHeader = Cookies.GetCookieHeader(absoluteUrl);
                if (!string.IsNullOrWhiteSpace(cookieHeader))
                {
                    requestBuilder.AddHeader(HeaderNames.Cookie, cookieHeader);
                }
            }

            private void UpdateCookies(HttpResponseMessage response, Uri absoluteUrl)
            {
                if (response.Headers.Contains(HeaderNames.SetCookie))
                {
                    var cookies = response.Headers.GetValues(HeaderNames.SetCookie);
                    foreach (var cookie in cookies)
                    {
                        Cookies.SetCookies(absoluteUrl, cookie);
                    }
                }
            }
        }
    }
}
