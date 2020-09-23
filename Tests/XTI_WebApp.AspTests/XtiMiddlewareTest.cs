using FakeWebApp.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.EF;
using XTI_Configuration.Extensions;
using XTI_WebApp.Api;
using XTI_WebApp.Extensions;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.AspTests
{
#pragma warning disable CS0162
    public class XtiMiddlewareTest
    {
        [Test]
        public async Task ShouldCreateSession()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await input.AppDbContext.Sessions.ToArrayAsync();
            Assert.That(sessions.Length, Is.EqualTo(1), "Should create session");
            Assert.That
            (
                sessions[0].UserAgent,
                Is.EqualTo("Mozilla/5.0,(Windows NT 6.1; WOW64),AppleWebKit/537.36,(KHTML, like Gecko),Chrome/28.0.1500.52,Safari/537.36,OPR/15.0.1147.100"),
                "Should create session with user agent from request"
            );
        }

        [Test]
        public async Task ShouldReuseSession()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await input.AppDbContext.Sessions.ToArrayAsync();
            Assert.That(sessions.Length, Is.EqualTo(1), "Should create session");
        }

        [Test]
        public async Task ShouldNotReuseSession_WhenSessionHasEnded()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await retrieveSessionsForToday(input);
            var session = sessions.First();
            input.Clock.Add(TimeSpan.FromHours(1));
            await session.End(input.Clock.Now());
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            sessions = await retrieveSessionsForToday(input);
            Assert.That(sessions.Length, Is.EqualTo(2), "Should create session");
        }

        private static async Task<AppSession[]> retrieveSessionsForToday(TestInput input)
        {
            return
            (
                await input.Factory.SessionRepository()
                            .RetrieveByDateRange(input.Clock.Today(), input.Clock.Today())
            )
            .ToArray();
        }

        [Test]
        public async Task ShouldUseSessionForAuthenticatedUser()
        {
            var input = await setup();
            var user = await input.Factory.UserRepository().RetrieveByUserName(new AppUserName("xartogg"));
            var session = await input.Factory.SessionRepository().Create(user, input.Clock.Now(), "", "");
            input.TestAuthOptions.IsEnabled = true;
            input.TestAuthOptions.Session = session;
            input.TestAuthOptions.User = user;
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await input.AppDbContext.Sessions.ToArrayAsync();
            Assert.That(sessions.Length, Is.EqualTo(1), "Should use session for authenticated user");
            Assert.That(sessions[0].UserID, Is.EqualTo(user.ID), "Should create session for authenticated user");
        }

        [Test]
        public async Task ShouldCreateSessionWithAnonymousUser()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await input.AppDbContext.Sessions.ToArrayAsync();
            var anonUser = await input.Factory.UserRepository().RetrieveByUserName(AppUserName.Anon);
            Assert.That(sessions[0].UserID, Is.EqualTo(anonUser.ID), "Should create session with anonymous user");
        }

        [Test]
        public async Task ShouldLogRequest()
        {
            var input = await setup();
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var sessions = await retrieveSessionsForToday(input);
            var requests = (await sessions[0].Requests()).ToArray();
            Assert.That(requests.Length, Is.EqualTo(1), "Should log request");
            Assert.That(requests[0].ResourceName, Is.EqualTo(XtiPath.Parse(uri)), "Should log resource key for request");
        }

        [Test]
        public async Task ShouldLogCurrentVersionWithRequest()
        {
            var input = await setup();
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var sessions = await retrieveSessionsForToday(input);
            var requests = (await sessions[0].Requests()).ToArray();
            var version = await requests[0].Version();
            Assert.That(version.ID, Is.EqualTo(input.CurrentVersion.ID));
        }

        [Test]
        public async Task ShouldLogExplicitVersionWithRequest()
        {
            var input = await setup();
            var explicitVersion = await input.App.StartNewPatch(input.Clock.Now());
            var uri = $"/Fake/v{explicitVersion.ID}/Controller1/Action1";
            await input.GetAsync(uri);
            var sessions = await retrieveSessionsForToday(input);
            var requests = (await sessions[0].Requests()).ToArray();
            var requestVersion = await requests[0].Version();
            Assert.That(requestVersion.ID, Is.EqualTo(explicitVersion.ID));
        }

        [Test]
        public async Task ShouldLogUnexpectedError()
        {
            Exception exception = null;
            var input = await setup(c =>
            {
                try
                {
                    throw new Exception("Testing critical error");
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                return Task.CompletedTask;
            });
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var session = (await retrieveSessionsForToday(input)).First();
            var request = (await session.Requests()).First();
            var events = (await request.Events()).ToArray();
            Assert.That(events.Length, Is.EqualTo(1), "Should log critical error");
            Assert.That(events[0].Severity, Is.EqualTo(AppEventSeverity.CriticalError), "Should log critical error");
            Assert.That(events[0].Caption, Is.EqualTo("An unexpected error occurred"), "Should log critical error");
            Assert.That(events[0].Message, Is.EqualTo(exception.Message), "Should log critical error");
            Assert.That(events[0].Detail, Is.EqualTo(exception.StackTrace), "Should log critical error");
        }

        [Test]
        public async Task ShouldReturnServerError_WhenAnUnexpectedErrorOccurs()
        {
            Exception exception = null;
            var input = await setup(c =>
            {
                try
                {
                    throw new Exception("Testing critical error");
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                return Task.CompletedTask;
            });
            var uri = "/Fake/Current/Controller1/Action1";
            var response = await input.GetAsync(uri);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            var result = await response.Content.ReadAsStringAsync();
            var expectedResult = JsonSerializer.Serialize(new[] { new ErrorModel("An unexpected error occurred") });
            Assert.That(result, Is.EqualTo(expectedResult), "Should return errors");
        }

        [Test]
        public async Task ShouldReturnBadRequestError400_WhenAValidationErrorOccurs()
        {
            var errors = new[] { new ErrorModel("Field is required") };
            var input = await setup(c =>
            {
                throw new ValidationFailedException(errors);
                return Task.CompletedTask;
            });
            var uri = "/Fake/Current/Controller1/Action1";
            var response = await input.GetAsync(uri);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var result = await response.Content.ReadAsStringAsync();
            var expectedResult = JsonSerializer.Serialize(errors);
            Assert.That(result, Is.EqualTo(expectedResult), "Should return errors");
        }

        [Test]
        public async Task ShouldReturnForbiddenError403_WhenAValidationErrorOccurs()
        {
            AccessDeniedException exception = null;
            var uri = "/Fake/Current/Controller1/Action1";
            var input = await setup(c =>
            {
                try
                {
                    throw new AccessDeniedException(XtiPath.Parse(uri));
                }
                catch (AccessDeniedException ex)
                {
                    exception = ex;
                    throw;
                }
                return Task.CompletedTask;
            });
            var response = await input.GetAsync(uri);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            var result = await response.Content.ReadAsStringAsync();
            var errors = new[] { new ErrorModel(exception.DisplayMessage) };
            var expectedResult = JsonSerializer.Serialize(errors);
            Assert.That(result, Is.EqualTo(expectedResult), "Should return errors");
        }

        [Test]
        public async Task ShouldReturnDisplayMessage_WhenAnAppErrorOccurs()
        {
            TestAppException exception = null;
            var uri = "/Fake/Current/Controller1/Action1";
            var input = await setup(c =>
            {
                try
                {
                    throw new TestAppException();
                }
                catch (TestAppException ex)
                {
                    exception = ex;
                    throw;
                }
                return Task.CompletedTask;
            });
            var response = await input.GetAsync(uri);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var result = await response.Content.ReadAsStringAsync();
            var errors = new[] { new ErrorModel(exception.DisplayMessage) };
            var expectedResult = JsonSerializer.Serialize(errors);
            Assert.That(result, Is.EqualTo(expectedResult), "Should return errors");
        }

        private sealed class TestAppException : AppException
        {
            public TestAppException() : base("Detailed message", "Message for user")
            {
            }
        }

        private async Task<TestInput> setup(Func<HttpContext, Task> mainApp = null)
        {
            if (mainApp == null)
            {
                mainApp = c =>
                {
                    c.Response.StatusCode = StatusCodes.Status200OK;
                    return Task.CompletedTask;
                };
            }
            var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices((context, services) =>
                        {
                            services.AddSingleton<TestAuthOptions>();
                            services
                                .AddAuthentication("Test")
                                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>
                                (
                                    "Test",
                                    options => { }
                                );
                            services.AddAuthorization(options =>
                            {
                                options.DefaultPolicy =
                                    new AuthorizationPolicyBuilder("Test")
                                        .RequireAuthenticatedUser()
                                        .Build();
                            });
                            services.AddFakesForXtiWebApp();
                            services.AddScoped(sp =>
                            {
                                var protector = sp.GetDataProtector("XTI_WEB_APP");
                                var httpClientAccessor = sp.GetService<IHttpContextAccessor>();
                                return new AnonClient(protector, httpClientAccessor);
                            });
                        })
                        .Configure(app =>
                        {
                            app.UseAuthentication();
                            app.UseAuthorization();
                            app.UseXti();
                            app.Run(c => mainApp(c));
                        });
                })
                .ConfigureAppConfiguration
                (
                    (hostingContext, config) => config.UseXtiConfiguration(hostingContext.HostingEnvironment.EnvironmentName, new string[] { })
                )
                .StartAsync();
            var factory = host.Services.GetService<AppFactory>();
            var setup = new AppSetup(factory);
            await setup.Run();
            await new FakeAppSetup(host.Services).Run();
            var clock = host.Services.GetService<Clock>();
            var app = await factory.AppRepository().App(FakeAppApi.AppKey);
            var version = await app.StartNewPatch(clock.Now());
            await version.Publishing();
            await version.Published();
            var input = new TestInput(host, app, version);
            await input.Factory.UserRepository().Add
            (
                new AppUserName("xartogg"), new FakeHashedPassword("password"), input.Clock.Now()
            );
            return input;
        }

        private sealed class TestInput
        {
            public TestInput(IHost host, App app, AppVersion currentVersion)
            {
                Host = host;
                AppDbContext = host.Services.GetService<AppDbContext>();
                Factory = host.Services.GetService<AppFactory>();
                Clock = (FakeClock)host.Services.GetService<Clock>();
                TestAuthOptions = host.Services.GetService<TestAuthOptions>();
                Cookies = new CookieContainer();
                App = app;
                CurrentVersion = currentVersion;
            }
            public IHost Host { get; }
            public CookieContainer Cookies { get; }
            public AppDbContext AppDbContext { get; }
            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public TestAuthOptions TestAuthOptions { get; }
            public App App { get; }
            public AppVersion CurrentVersion { get; }

            public async Task<HttpResponseMessage> GetAsync(string relativeUrl)
            {
                var testServer = Host.GetTestServer();
                testServer.BaseAddress = new Uri("https://localhost");
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
