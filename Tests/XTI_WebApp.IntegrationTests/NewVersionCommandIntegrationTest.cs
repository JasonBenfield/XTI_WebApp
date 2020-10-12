﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.EF;
using XTI_Configuration.Extensions;
using XTI_Secrets;
using XTI_Version;
using XTI_WebApp.Extensions;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.IntegrationTests
{
    public sealed class NewVersionCommandIntegrationTest
    {
        [Test]
        public async Task ShouldCreateNewPatch()
        {
            var input = await setup();
            input.Options.VersionType = AppVersionType.Values.Patch.DisplayText;
            var newVersion = await execute(input);
            Assert.That(newVersion?.IsPatch(), Is.True, "Should start new patch");
        }

        [Test]
        public async Task ShouldCreateNewMinorVersion()
        {
            var input = await setup();
            input.Options.VersionType = AppVersionType.Values.Minor.DisplayText;
            var newVersion = await execute(input);
            Assert.That(newVersion?.IsMinor(), Is.True, "Should start new minor version");
        }

        [Test]
        public async Task ShouldCreateNewMajorVersion()
        {
            var input = await setup();
            input.Options.VersionType = AppVersionType.Values.Major.DisplayText;
            var newVersion = await execute(input);
            Assert.That(newVersion?.IsMajor(), Is.True, "Should start new major version");
        }

        private async Task<TestInput> setup()
        {
            var hostEnv = new FakeHostEnvironment { EnvironmentName = "Test" };
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", hostEnv.EnvironmentName);
            var services = new ServiceCollection();
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.UseXtiConfiguration("Test", new string[] { });
            var configuration = configurationBuilder.Build();
            services.AddScoped<IHostEnvironment>(sp => hostEnv);
            services.AddWebAppServices(configuration);
            services.AddFileSecretCredentials();
            services.AddScoped<ManageVersionCommand>();
            services.AddScoped<AppDbReset>();
            var sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            var appDbReset = sp.GetService<AppDbReset>();
            await appDbReset.Run();
            await new AppSetup(factory).Run();
            var app = await factory.Apps().AddApp(new AppKey("Fake"), AppType.Values.WebApp, "Fake", DateTime.UtcNow);
            var input = new TestInput(sp, app);
            return input;
        }

        private Task<AppVersion> execute(TestInput input)
        {
            var command = input.Command();
            return command.Execute(input.Options);
        }

        private sealed class TestInput
        {
            private readonly ServiceProvider sp;

            public TestInput(ServiceProvider sp, App app)
            {
                this.sp = sp;
                Factory = sp.GetService<AppFactory>();
                App = app;
                Options = new ManageVersionOptions
                {
                    Command = "New",
                    AppKey = app.Key().Value,
                    VersionType = AppVersionType.Values.Patch.DisplayText
                };
            }

            public AppFactory Factory { get; }
            public App App { get; }
            public ManageVersionOptions Options { get; }

            public ManageVersionCommand Command() => sp.GetService<ManageVersionCommand>();
        }
    }
}
