using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_Version;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.Tests
{
    public sealed class ManageVersionTester
    {
        private IServiceProvider sp;

        public AppFactory Factory { get; private set; }
        public App App { get; private set; }
        public FakeClock Clock { get; private set; }
        public ManageVersionOptions Options { get; private set; }

        public async Task Setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            services.AddSingleton<ManageVersionCommand>();
            services.AddFakeSecretCredentials();
            sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            var setup = new AppSetup(factory);
            await setup.Run();
            var app = await factory.Apps().AddApp(new AppKey("Fake"), AppType.Values.WebApp, "Fake", DateTime.UtcNow);
            Factory = sp.GetService<AppFactory>();
            App = app;
            Clock = sp.GetService<FakeClock>();
            Options = new ManageVersionOptions
            {
                Command = "New",
                BranchName = "",
                AppKey = app.Key().Value,
                VersionType = AppVersionType.Values.Patch.DisplayText,
            };
        }

        public Task<AppVersion> Execute()
        {
            var command = Command();
            return command.Execute(Options);
        }

        public ManageVersionCommand Command() => sp.GetService<ManageVersionCommand>();
    }
}
