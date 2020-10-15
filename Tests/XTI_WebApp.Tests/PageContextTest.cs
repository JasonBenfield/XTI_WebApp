using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core.Fakes;
using XTI_WebApp.Extensions;
using XTI_WebApp.Fakes;

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
            var input = await setup();
            input.AppOptions.BaseUrl = "https://webapps.xartogg.com";
            var pageContext = await execute(input);
            Assert.That(pageContext.BaseUrl, Is.EqualTo(input.AppOptions.BaseUrl), "Should set app title");
        }

        [Test]
        public async Task ShouldSetEnvironmentName()
        {
            var input = await setup();
            input.HostEnvironment.EnvironmentName = "Staging";
            var pageContext = await execute(input);
            Assert.That(pageContext.EnvironmentName, Is.EqualTo(input.HostEnvironment.EnvironmentName), "Should set environment name");
        }

        [Test]
        public async Task ShouldSetUserName()
        {
            var input = await setup();
            var user = await input.UserContext.User();
            var pageContext = await execute(input);
            Assert.That(pageContext.IsAuthenticated, Is.True, "Should be authenticated");
            Assert.That(pageContext.UserName, Is.EqualTo(user.UserName().Value), "Should set user name");
        }

        [Test]
        public async Task ShouldSetUserNameToBlankForAnon()
        {
            var input = await setup();
            var anonUser = await input.Factory.Users().User(AppUserName.Anon);
            input.UserContext.SetUser(anonUser);
            var pageContext = await execute(input);
            Assert.That(pageContext.IsAuthenticated, Is.False, "Should not be authenticated");
            Assert.That(pageContext.UserName, Is.EqualTo(""), "Should set user name to blank for anon");
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

        private async Task<TestInput> setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            var sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            await new AppSetup(factory).Run();
            var clock = sp.GetService<FakeClock>();
            var input = new TestInput(sp);
            var app = await factory.Apps().AddApp(new AppKey("Fake"), AppType.Values.WebApp, "Fake", clock.Now());
            input.AppContext.SetApp(app);
            var user = await factory.Users().Add(new AppUserName("someone"), new FakeHashedPassword("Password"), clock.Now());
            input.UserContext.SetUser(user);
            return input;
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp)
            {
                Factory = sp.GetService<AppFactory>();
                Clock = sp.GetService<FakeClock>();
                AppContext = (FakeAppContext)sp.GetService<IAppContext>();
                UserContext = (FakeUserContext)sp.GetService<IUserContext>();
                HostEnvironment = (FakeHostEnvironment)sp.GetService<IHostEnvironment>();
                AppOptions = sp.GetService<IOptions<AppOptions>>().Value;
                PageContext = (PageContext)sp.GetService<IPageContext>();

            }

            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public FakeAppContext AppContext { get; }
            public FakeUserContext UserContext { get; }
            public FakeHostEnvironment HostEnvironment { get; }
            public AppOptions AppOptions { get; }
            public PageContext PageContext { get; }
        }
    }
}
