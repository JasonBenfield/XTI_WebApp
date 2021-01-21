using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_App.Fakes;
using XTI_Core.Fakes;
using XTI_WebApp.Api;
using XTI_WebApp.Fakes;
using XTI_WebApp.TestFakes;

namespace XTI_WebApp.Tests
{
    public sealed class PageContextTest
    {
        [Test]
        public async Task ShouldSetAppTitle()
        {
            var input = await setup();
            var pageContext = await execute(input);
            var app = await input.AppContext.App();
            Assert.That(pageContext.AppTitle, Is.EqualTo(app.Title), "Should set app title");
        }

        [Test]
        public async Task ShouldSetBaseUrl()
        {
            var baseUrl = "https://webapps.xartogg.com";
            var input = await setup(baseUrl: baseUrl);
            var pageContext = await execute(input);
            Assert.That(pageContext.BaseUrl, Is.EqualTo(baseUrl), "Should set app title");
        }

        [Test]
        public async Task ShouldSetEnvironmentName()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Staging");
            var input = await setup();
            var pageContext = await execute(input);
            Assert.That(pageContext.EnvironmentName, Is.EqualTo("Staging"), "Should set environment name");
        }

        [Test]
        public async Task ShouldSetUserName()
        {
            var input = await setup();
            var user = await input.Factory.Users().User(new AppUserName("someone"));
            input.HttpContextAccessor.HttpContext = new DefaultHttpContext
            {
                User = new FakeHttpUser().Create("", user)
            };
            var pageContext = await execute(input);
            Assert.That(pageContext.IsAuthenticated, Is.True, "Should be authenticated");
            Assert.That(pageContext.UserName, Is.EqualTo(user.UserName().Value), "Should set user name");
        }

        [Test]
        public async Task ShouldSetUserNameToBlankForAnon()
        {
            var input = await setup();
            input.HttpContextAccessor.HttpContext = new DefaultHttpContext();
            var pageContext = await execute(input);
            Assert.That(pageContext.IsAuthenticated, Is.False, "Should not be authenticated");
            Assert.That(pageContext.UserName, Is.EqualTo(""), "Should set user name to blank for anon");
        }

        [Test]
        public async Task ShouldSetCacheBustToCurrentVersion()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
            var input = await setup();
            var pageContext = await execute(input);
            var app = await input.Factory.Apps().App(FakeInfo.AppKey);
            var currentVersion = await app.CurrentVersion();
            Assert.That(pageContext?.CacheBust, Is.EqualTo(currentVersion.Key().DisplayText), "Should set cacheBust to current version");
        }

        [Test]
        public async Task ShouldNotSetCacheBust_WhenVersionIsNotCurrent()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
            var input = await setup(new AppVersionKey(2));
            var pageContext = await execute(input);
            Assert.That(pageContext?.CacheBust, Is.Null, "Should not set cacheBust when version is null");
        }

        private static async Task<PageContextRecord> execute(TestInput input)
        {
            var serialized = await input.PageContext.Serialize();
            var pageContext = JsonSerializer.Deserialize<PageContextRecord>(serialized);
            return pageContext;
        }

        private class PageContextRecord
        {
            public string BaseUrl { get; set; }
            public string CacheBust { get; set; }
            public string AppTitle { get; set; }
            public string PageTitle { get; set; }
            public string UserName { get; set; }
            public bool IsAuthenticated { get; set; }
            public string EnvironmentName { get; set; }
        }

        private async Task<TestInput> setup(AppVersionKey versionKey = null, string baseUrl = "https://www.xartogg.com")
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration
                (
                    config =>
                    {
                        config.AddInMemoryCollection
                        (
                            new[]
                            {
                                KeyValuePair.Create("App:BaseUrl", baseUrl)
                            }
                        );
                    }
                )
                .ConfigureServices
                (
                    (hostContext, services) =>
                    {
                        services.AddFakesForXtiWebApp(hostContext.Configuration);
                        services.AddSingleton(sp => FakeInfo.AppKey);
                        services.AddSingleton
                        (
                            sp => new XtiPath
                            (
                                FakeInfo.AppKey.Name.DisplayText,
                                versionKey ?? AppVersionKey.Current.DisplayText,
                                "",
                                "",
                                ModifierKey.Default
                            )
                        );
                    }
                )
                .Build();
            var scope = host.Services.CreateScope();
            var sp = scope.ServiceProvider;
            var factory = sp.GetService<AppFactory>();
            var clock = sp.GetService<FakeClock>();
            await new FakeAppSetup(factory, clock).Run();
            var input = new TestInput(sp);
            await factory.Apps().App(FakeInfo.AppKey);
            await factory.Users().Add(new AppUserName("someone"), new FakeHashedPassword("Password"), clock.Now());
            return input;
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp)
            {
                Factory = sp.GetService<AppFactory>();
                AppContext = sp.GetService<IAppContext>();
                HttpContextAccessor = sp.GetService<IHttpContextAccessor>();
                PageContext = (PageContext)sp.GetService<IPageContext>();
            }

            public AppFactory Factory { get; }
            public IAppContext AppContext { get; }
            public IHttpContextAccessor HttpContextAccessor { get; }
            public PageContext PageContext { get; }
        }
    }
}
