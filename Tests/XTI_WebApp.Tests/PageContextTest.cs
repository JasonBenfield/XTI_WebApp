using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
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
            Assert.That(pageContext.AppTitle, Is.EqualTo(input.App.Title), "Should set app title");
        }

        [Test]
        public async Task ShouldSetBaseUrl()
        {
            var input = await setup();
            input.AppOptions.BaseUrl = "https://webapps.xartogg.com";
            var pageContext = await execute(input);
            Assert.That(pageContext.BaseUrl, Is.EqualTo(input.AppOptions.BaseUrl), "Should set app title");
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
        }

        private async Task<TestInput> setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            var sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            var setup = new AppSetup(factory);
            await setup.Run();
            var clock = sp.GetService<FakeClock>();
            var app = await factory.Apps().AddApp(new AppKey("Fake"), "Fake", clock.Now());
            var appContext = (FakeAppContext)sp.GetService<IAppContext>();
            appContext.SetApp(app);
            return new TestInput(sp, app);
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp, App app)
            {
                Factory = sp.GetService<AppFactory>();
                Clock = sp.GetService<FakeClock>();
                App = app;
                PageContext = sp.GetService<PageContext>();
                AppOptions = sp.GetService<IOptions<AppOptions>>().Value;

            }

            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public App App { get; }
            public PageContext PageContext { get; }
            public AppOptions AppOptions { get; }
        }
    }
}
