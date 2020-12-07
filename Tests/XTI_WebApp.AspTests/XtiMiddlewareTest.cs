using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Configuration.Extensions;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_TempLog;
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
            var session = await getFirstStartSession(input);
            Assert.That
            (
                session.UserAgent,
                Is.EqualTo("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.52 Safari/537.36 OPR/15.0.1147.100"),
                "Should create session with user agent from request"
            );
        }

        [Test]
        public async Task ShouldLogRequesterWithSession()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var session = await getFirstStartSession(input);
            Assert.That(string.IsNullOrWhiteSpace(session.RequesterKey), Is.False, "Should log requester with session");
        }

        private static async Task<StartSessionModel> getFirstStartSession(TestInput input)
        {
            var sessions = await getStartSessions(input);
            Assert.That(sessions.Length, Is.EqualTo(1), "Should create session");
            return sessions[0];
        }

        private static async Task<StartSessionModel[]> getStartSessions(TestInput input)
        {
            var sessionFiles = input.CurrentAction.TempLog.StartSessionFiles(input.Clock.Now().AddMinutes(1));
            var sessions = new List<StartSessionModel>();
            foreach (var sessionFile in sessionFiles)
            {
                var serializedSession = await sessionFile.Read();
                var session = JsonSerializer.Deserialize<StartSessionModel>(serializedSession);
                sessions.Add(session);
            }
            return sessions.ToArray();
        }

        [Test]
        public async Task ShouldReuseRequesterKeyWithNewSession()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            input.Clock.Set(input.Clock.Now().AddHours(4).AddMinutes(1));
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await getStartSessions(input);
            Assert.That(sessions.Length, Is.EqualTo(2));
            Assert.That(sessions[1].RequesterKey, Is.EqualTo(sessions[0].RequesterKey), "Should reuse requester key");
        }

        [Test]
        public async Task ShouldNotStartAnonSession_WhenAnonSessionWasAlreadyStarted()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await getStartSessions(input);
            Assert.That(sessions.Length, Is.EqualTo(1), "Should not start new session when anon session has already started");
        }

        [Test]
        public async Task ShouldNotReuseAnonymousSession_WhenSessionHasExpired()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            input.Clock.Set(input.Clock.Now().AddHours(4).AddMinutes(1));
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessions = await getStartSessions(input);
            Assert.That(sessions.Length, Is.EqualTo(2), "Should not reuse anonymous session when it has expired");
            Assert.That(sessions[1].SessionKey, Is.Not.EqualTo(sessions[0].SessionKey), "Should not reuse anonymous session when it has expired");
        }

        [Test]
        public async Task ShouldNotStartNewSession_WhenSessionHasAlreadyBeenStarted()
        {
            var input = await setup();
            var user = await input.Factory.Users().User(new AppUserName("xartogg"));
            input.TestAuthOptions.IsEnabled = true;
            input.TestAuthOptions.SessionKey = Guid.NewGuid().ToString("N");
            input.TestAuthOptions.User = user;
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var sessionFiles = input.CurrentAction.TempLog.StartSessionFiles(input.Clock.Now().AddMinutes(1)).ToArray();
            Assert.That(sessionFiles.Length, Is.EqualTo(0), "Should not start session when session key exists");
        }

        [Test]
        public async Task ShouldCreateSessionWithAnonymousUser()
        {
            var input = await setup();
            await input.GetAsync("/Fake/Current/Controller1/Action1");
            var anonUser = await input.Factory.Users().User(AppUserName.Anon);
            var sessionFiles = input.CurrentAction.TempLog.StartSessionFiles(input.Clock.Now().AddMinutes(1)).ToArray();
            Assert.That(sessionFiles.Length, Is.EqualTo(1), "Should use session for authenticated user");
            var serializedSession = await sessionFiles[0].Read();
            var session = JsonSerializer.Deserialize<StartSessionModel>(serializedSession);
            Assert.That(session.UserName, Is.EqualTo(anonUser.UserName().Value), "Should create session with anonymous user");
        }

        [Test]
        public async Task ShouldLogRequest()
        {
            var input = await setup();
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var request = await getStartRequest(input);
            Assert.That(request.Path, Is.EqualTo(uri), "Should log resource key for request");
        }

        [Test]
        public async Task ShouldLogEndOfRequest()
        {
            var input = await setup();
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var requestFiles = input.CurrentAction.TempLog.EndRequestFiles(input.Clock.Now().AddMinutes(1)).ToArray();
            Assert.That(requestFiles.Length, Is.EqualTo(1), "Should log end of request");
        }

        [Test]
        public async Task ShouldLogCurrentVersionWithRequest()
        {
            var input = await setup();
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var request = await getStartRequest(input);
            var path = XtiPath.Parse(request.Path);
            Assert.That(path.Version, Is.EqualTo(AppVersionKey.Current), "Should log current version");
        }

        [Test]
        public async Task ShouldLogExplicitVersionWithRequest()
        {
            var input = await setup();
            var app = await input.Factory.Apps().App(FakeAppKey.AppKey);
            var explicitVersion = await app.StartNewPatch(input.Clock.Now());
            var uri = $"/Fake/{explicitVersion.Key().Value}/Controller1/Action1";
            await input.GetAsync(uri);
            var request = await getStartRequest(input);
            var path = XtiPath.Parse(request.Path);
            Assert.That(path.Version, Is.EqualTo(explicitVersion.Key()), "Should log explicit version");
        }

        private static async Task<StartRequestModel> getStartRequest(TestInput input)
        {
            var requestFiles = input.CurrentAction.TempLog.StartRequestFiles(input.Clock.Now().AddMinutes(1)).ToArray();
            Assert.That(requestFiles.Length, Is.EqualTo(1), "Should log end of request");
            var serializedRequest = await requestFiles[0].Read();
            var request = JsonSerializer.Deserialize<StartRequestModel>(serializedRequest);
            return request;
        }

        [Test]
        public async Task ShouldLogUnexpectedError()
        {
            Exception exception = null;
            var input = await setup();
            input.CurrentAction.Action = c =>
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
            };
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var evt = await getEvent(input);
            Assert.That(evt.Severity, Is.EqualTo(AppEventSeverity.Values.CriticalError.Value), "Should log critical error");
            Assert.That(evt.Caption, Is.EqualTo("An unexpected error occurred"), "Should log critical error");
            Assert.That(evt.Message, Is.EqualTo(exception.Message), "Should log critical error");
            Assert.That(evt.Detail, Is.EqualTo(exception.StackTrace), "Should log critical error");
        }

        private static async Task<LogEventModel> getEvent(TestInput input)
        {
            var eventFiles = input.CurrentAction.TempLog.LogEventFiles(input.Clock.Now().AddMinutes(1)).ToArray();
            Assert.That(eventFiles.Length, Is.EqualTo(1), "Should log event");
            var serializedEvent = await eventFiles[0].Read();
            var evt = JsonSerializer.Deserialize<LogEventModel>(serializedEvent);
            return evt;
        }

        [Test]
        public async Task ShouldLogValidationError()
        {
            Exception exception = null;
            var input = await setup();
            input.CurrentAction.Action = c =>
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
            };
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var evt = await getEvent(input);
            Assert.That(evt.Severity, Is.EqualTo(AppEventSeverity.Values.ValidationFailed.Value), "Should log validation failed");
            Assert.That(evt.Caption, Is.EqualTo("Validation Failed"), "Should log validation failed");
            Assert.That(evt.Message, Is.EqualTo("Validation failed with the following errors:\r\nUser name is required"), "Should log validation failed");
            Assert.That(evt.Detail, Is.EqualTo(exception.StackTrace), "Should log validation failed");
        }

        [Test]
        public async Task ShouldLogAppError()
        {
            Exception exception = null;
            var input = await setup();
            input.CurrentAction.Action = c =>
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
            };
            var uri = "/Fake/Current/Controller1/Action1";
            await input.GetAsync(uri);
            var evt = await getEvent(input);
            Assert.That(evt.Severity, Is.EqualTo(AppEventSeverity.Values.AppError.Value), "Should log app error");
            Assert.That(evt.Caption, Is.EqualTo("Display Message"), "Should log app error");
            Assert.That(evt.Message, Is.EqualTo("Full Message"), "Should log app error");
            Assert.That(evt.Detail, Is.EqualTo(exception.StackTrace), "Should log app error");
        }

        [Test]
        public async Task ShouldLogAccessDenied()
        {
            Exception exception = null;
            var uri = "/Fake/Current/Controller1/Action1";
            var input = await setup();
            input.CurrentAction.Action = c =>
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
            };
            await input.GetAsync(uri);
            var evt = await getEvent(input);
            Assert.That(evt.Severity, Is.EqualTo(AppEventSeverity.Values.AccessDenied.Value), "Should log access denied");
            Assert.That(evt.Caption, Is.EqualTo("Access Denied"), "Should log access denied");
            Assert.That(evt.Message, Is.EqualTo("Access denied to Fake/Current/Controller1/Action1"), "Should log access denied");
            Assert.That(evt.Detail, Is.EqualTo(exception.StackTrace), "Should log access denied");
        }

        [Test]
        public async Task ShouldReturnServerError_WhenAnUnexpectedErrorOccurs()
        {
            Exception exception = null;
            var input = await setup();
            input.CurrentAction.Action = c =>
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
            };
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
            var input = await setup();
            input.CurrentAction.Action = c =>
            {
                throw new ValidationFailedException(errors);
                return Task.CompletedTask;
            };
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
            var input = await setup();
            input.CurrentAction.Action = c =>
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
            };
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
            var input = await setup();
            input.CurrentAction.Action = c =>
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
            };
            var response = await input.GetAsync(uri);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var result = await response.Content.ReadAsStringAsync();
            var errors = new[] { new ErrorModel(exception.DisplayMessage) };
            var expectedResult = JsonSerializer.Serialize(new ResultContainer<ErrorModel[]>(errors));
            Assert.That(result, Is.EqualTo(expectedResult), "Should return errors");
        }

        private sealed class TestAppException : AppException
        {
            public TestAppException() : base("Detailed message", "Message for user")
            {
            }
        }

        private sealed class CurrentAction
        {
            public CurrentAction()
            {
                Action = (c) =>
                {
                    c.Response.StatusCode = StatusCodes.Status200OK;
                    return Task.CompletedTask;
                };
            }
            public TempLog TempLog { get; set; }
            public Func<HttpContext, Task> Action { get; set; }
        }

        private async Task<TestInput> setup()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
            var hostBuilder = new HostBuilder();
            var host = await hostBuilder
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices((context, services) =>
                        {
                            services.AddSingleton<CurrentAction>();
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
                            services.AddFakesForXtiWebApp(context.Configuration);
                            services.AddSingleton(sp => FakeAppKey.AppKey);
                            services.AddSingleton<AppFactory>();
                            services.AddSingleton<IAnonClient, FakeAnonClient>();
                            services.AddScoped<FakeAppSetup>();
                        })
                        .Configure(app =>
                        {
                            app.UseAuthentication();
                            app.UseAuthorization();
                            app.UseXti();
                            app.Run(async (c) =>
                            {
                                var currentAction = c.RequestServices.GetService<CurrentAction>();
                                currentAction.TempLog = c.RequestServices.GetService<TempLog>();
                                await currentAction.Action(c);
                            });
                        });
                })
                .ConfigureAppConfiguration
                (
                    (hostingContext, config) =>
                    {
                        config.UseXtiConfiguration(hostingContext.HostingEnvironment, new string[] { });
                    }
                )
                .StartAsync();
            var factory = host.Services.GetService<AppFactory>();
            var clock = host.Services.GetService<Clock>();
            await new FakeAppSetup(factory, clock).Run();
            return new TestInput(host);
        }

        private sealed class TestInput
        {
            public TestInput(IHost host)
            {
                Host = host;
                Factory = host.Services.GetService<AppFactory>();
                Clock = (FakeClock)host.Services.GetService<Clock>();
                TestAuthOptions = host.Services.GetService<TestAuthOptions>();
                Cookies = new CookieContainer();
                CurrentAction = host.Services.GetService<CurrentAction>();
            }
            public IHost Host { get; }
            public CookieContainer Cookies { get; }
            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public TestAuthOptions TestAuthOptions { get; }
            public CurrentAction CurrentAction { get; }

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
