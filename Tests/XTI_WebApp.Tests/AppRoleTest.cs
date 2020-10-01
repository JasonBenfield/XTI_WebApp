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
    public sealed class AppRoleTest
    {
        [Test]
        public async Task ShouldAddRoleToApp()
        {
            var input = await setup();
            var adminRoleName = new AppRoleName("Admin");
            await input.App.AddRole(adminRoleName);
            var roles = (await input.App.Roles()).ToArray();
            Assert.That(roles.Length, Is.EqualTo(1), "Should add role to app");
            Assert.That(roles[0].Name(), Is.EqualTo(adminRoleName), "Should add role to app");
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
            var app = await factory.AppRepository().AddApp(new AppKey("Fake"), clock.Now());
            return new TestInput(factory, app);
        }

        private sealed class TestInput
        {
            public TestInput(AppFactory factory, App app)
            {
                Factory = factory;
                App = app;
            }

            public AppFactory Factory { get; }
            public App App { get; }
        }
    }
}
