using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
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
using XTI_App.Api;
using XTI_App.EF;
using XTI_Configuration.Extensions;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_WebApp.Extensions;
using XTI_WebApp.Fakes;
using XTI_WebApp.TestFakes;

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
        public async Task ShouldLogRequesterWithSession()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await input.AppDbContext.Sessions.ToArrayAsync();
            Assert.That(string.IsNullOrWhiteSpace(sessions[0].RequesterKey), Is.False, "Should log requester with session");
        }

        [Test]
        public async Task ShouldReuseRequesterKeyWithNewSession()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await retrieveSessionsForToday(input);
            var session = sessions.First();
            input.Clock.Add(TimeSpan.FromHours(1));
            await session.End(input.Clock.Now());
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessionRecords = await input.AppDbContext.Sessions.ToArrayAsync();
            Assert.That(sessionRecords[0].RequesterKey, Is.EqualTo(sessionRecords[1].RequesterKey), "Should reuse requester key with new session");

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
                await input.Factory.Sessions()
                    .SessionsByTimeRange(input.Clock.Now().AddDays(-1), input.Clock.Now().AddDays(1))
            )
            .ToArray();
        }

        [Test]
        public async Task ShouldUseSessionForAuthenticatedUser()
        {
            var input = await setup();
            var user = await input.Factory.Users().User(new AppUserName("xartogg"));
            var session = await input.Factory.Sessions().Create(Guid.NewGuid().ToString("N"), user, input.Clock.Now(), "", "", "");
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
            var anonUser = await input.Factory.Users().User(AppUserName.Anon);
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
        public async Task ShouldLogEndOfRequest()
        {
            var input = await setup();
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var sessions = await retrieveSessionsForToday(input);
            var requests = (await sessions[0].Requests()).ToArray();
            Assert.That(requests[0].HasEnded(), Is.True, "Should log end of request");
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
            var uri = $"/Fake/V{explicitVersion.ID}/Controller1/Action1";
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
            Assert.That(events[0].Severity, Is.EqualTo(AppEventSeverity.Values.CriticalError), "Should log critical error");
            Assert.That(events[0].Caption, Is.EqualTo("An unexpected error occurred"), "Should log critical error");
            Assert.That(events[0].Message, Is.EqualTo(exception.Message), "Should log critical error");
            Assert.That(events[0].Detail, Is.EqualTo(exception.StackTrace), "Should log critical error");
        }

        [Test]
        public async Task ShouldLogValidationError()
        {
            Exception exception = null;
            var input = await setup(c =>
            {
                try
                {
                    throw new ValidationFailedException(new[] { new ErrorModel("User name is required") });
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
            Assert.That(events.Length, Is.EqualTo(1), "Should log validation failed");
            Assert.That(events[0].Severity, Is.EqualTo(AppEventSeverity.Values.ValidationFailed), "Should log validation failed");
            Assert.That(events[0].Caption, Is.EqualTo("Validation Failed"), "Should log validation failed");
            Assert.That(events[0].Message, Is.EqualTo("Validation failed with the following errors:\r\nUser name is required"), "Should log validation failed");
            Assert.That(events[0].Detail, Is.EqualTo(exception.StackTrace), "Should log validation failed");
        }

        [Test]
        public async Task ShouldLogAppError()
        {
            Exception exception = null;
            var input = await setup(c =>
            {
                try
                {
                    throw new AppException("Full Message", "Display Message");
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
            Assert.That(events.Length, Is.EqualTo(1), "Should log app error");
            Assert.That(events[0].Severity, Is.EqualTo(AppEventSeverity.Values.AppError), "Should log app error");
            Assert.That(events[0].Caption, Is.EqualTo("Display Message"), "Should log app error");
            Assert.That(events[0].Message, Is.EqualTo("Full Message"), "Should log app error");
            Assert.That(events[0].Detail, Is.EqualTo(exception.StackTrace), "Should log app error");
        }

        [Test]
        public async Task ShouldLogAccessDenied()
        {
            Exception exception = null;
            var uri = "/Fake/Current/Controller1/Action1";
            var input = await setup(c =>
            {
                try
                {
                    throw new AccessDeniedException(XtiPath.Parse(uri));
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                return Task.CompletedTask;
            });
            await input.GetAsync(uri);
            var session = (await retrieveSessionsForToday(input)).First();
            var request = (await session.Requests()).First();
            var events = (await request.Events()).ToArray();
            Assert.That(events.Length, Is.EqualTo(1), "Should log access denied");
            Assert.That(events[0].Severity, Is.EqualTo(AppEventSeverity.Values.AccessDenied), "Should log access denied");
            Assert.That(events[0].Caption, Is.EqualTo("Access Denied"), "Should log access denied");
            Assert.That(events[0].Message, Is.EqualTo("Access denied to Fake/Current/Controller1/Action1"), "Should log access denied");
            Assert.That(events[0].Detail, Is.EqualTo(exception.StackTrace), "Should log access denied");
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
            var expectedResult = JsonSerializer.Serialize
            (
                new ResultContainer<ErrorModel[]>
                (
                    new[] { new ErrorModel("An unexpected error occurred") }
                )
            );
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
            var expectedResult = JsonSerializer.Serialize(new ResultContainer<ErrorModel[]>(errors));
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
            var expectedResult = JsonSerializer.Serialize(new ResultContainer<ErrorModel[]>(errors));
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
            var expectedResult = JsonSerializer.Serialize(new ResultContainer<ErrorModel[]>(errors));
            Assert.That(result, Is.EqualTo(expectedResult), "Should return errors");
        }

        [Test]
        public async Task ShouldSetCacheBustToCurrentVersion()
        {
            PageContext pageContext = null;
            var input = await setup(async (context) =>
            {
                pageContext = (PageContext)context.RequestServices.GetService<IPageContext>();
                await pageContext.Serialize();
            });
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            Assert.That(pageContext?.CacheBust, Is.EqualTo($"V{input.CurrentVersion.ID}"), "Should set cacheBust to current version");
        }

        [Test]
        public async Task ShouldNotSetCacheBust_WhenVersionIsNotCurrent()
        {
            PageContext pageContext = null;
            var input = await setup(async (context) =>
            {
                pageContext = context.RequestServices.GetService<PageContext>();
                await pageContext.Serialize();
            });
            var uri = $"/Fake/V{input.CurrentVersion.ID}/Controller1/Action1";
            await input.GetAsync(uri);
            Assert.That(pageContext?.CacheBust, Is.Null, "Should not set cacheBust when version is null");
        }

        [Test]
        public async Task ShouldCacheCurrentVersion()
        {
            IAppVersion versionFromContext = null;
            var input = await setup(async (context) =>
            {
                var appContext = context.RequestServices.GetService<IAppContext>();
                var app = await appContext.App();
                versionFromContext = await app.CurrentVersion();
            });
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var version = await input.App.StartNewMajorVersion(DateTime.UtcNow);
            await version.Publishing();
            await version.Published();
            await input.GetAsync(uri);
            Assert.That(versionFromContext?.ID, Is.EqualTo(input.CurrentVersion.ID), "Should cache current version");
        }

        [Test]
        public async Task ShouldCacheUserRoles()
        {
            IAppUserRole[] userRoles = new IAppUserRole[] { };
            var input = await setup(async (context) =>
            {
                var userContext = context.RequestServices.GetService<IUserContext>();
                var user = await userContext.User();
                var appContext = context.RequestServices.GetService<IAppContext>();
                var app = await appContext.App();
                userRoles = (await user.RolesForApp(app)).ToArray();
            });
            var adminRole = await input.App.AddRole(new AppRoleName("Admin"));
            var managerRole = await input.App.AddRole(new AppRoleName("Manager"));
            var user = await input.Factory.Users().User(new AppUserName("xartogg"));
            var userAdminRole = await user.AddRole(adminRole);
            await user.AddRole(managerRole);
            var session = await input.Factory.Sessions().Create(Guid.NewGuid().ToString("N"), user, input.Clock.Now(), "", "", "");
            input.TestAuthOptions.IsEnabled = true;
            input.TestAuthOptions.Session = session;
            input.TestAuthOptions.User = user;
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            await user.RemoveRole(userAdminRole);
            await input.GetAsync(uri);
            Assert.That(userRoles.Length, Is.EqualTo(2), "Should cache user roles");
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
            var hostBuilder = new HostBuilder();
            var host = await hostBuilder
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
                            services.AddXtiContextServices();
                            services.AddScoped<FakeAppSetup>();
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
            await new AppSetup(factory).Run();
            var setup = host.Services.GetService<FakeAppSetup>();
            await setup.Run();
            return new TestInput(host, setup.App, setup.CurrentVersion);
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
                HostEnvironment = (FakeHostEnvironment)host.Services.GetService<IHostEnvironment>();
                HostEnvironment.EnvironmentName = "Production";
            }
            public IHost Host { get; }
            public CookieContainer Cookies { get; }
            public AppDbContext AppDbContext { get; }
            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public TestAuthOptions TestAuthOptions { get; }
            public App App { get; }
            public AppVersion CurrentVersion { get; }
            public FakeHostEnvironment HostEnvironment { get; }

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
