using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XTI_App;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.Tests
{
    public sealed class AppVersionTest
    {
        [Test]
        public async Task ShouldStartNewVersionForApp()
        {
            var input = await setup();
            var version = await input.App.StartNewVersion(input.Clock.Now());
            var versions = (await input.App.Versions()).ToArray();
            Assert.That(versions.Length, Is.EqualTo(1), "Should add version to app");
            Assert.That(versions[0].ID, Is.EqualTo(version.ID));
        }

        private async Task<TestInput> setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            var sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            var setup = new AppSetup(factory);
            await setup.Run();
            var clock = (FakeClock)sp.GetService<Clock>();
            var app = await factory.AppRepository().AddApp(FakeAppApi.AppKey, clock.Now());
            return new TestInput(factory, clock, app);
        }

        private sealed class TestInput
        {
            public TestInput(AppFactory factory, FakeClock clock, App app)
            {
                Factory = factory;
                Clock = clock;
                App = app;
            }

            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public App App { get; }
        }
    }
}
