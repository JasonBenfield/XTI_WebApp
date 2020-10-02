using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.EF;
using XTI_Configuration.Extensions;
using XTI_Version;
using XTI_Version.Octo;
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
            input.Options.Type = AppVersionType.Patch.DisplayText;
            var newVersion = await execute(input);
            Assert.That(newVersion?.IsPatch(), Is.True, "Should start new patch");
        }

        [Test]
        public async Task ShouldCreateNewMinorVersion()
        {
            var input = await setup();
            input.Options.Type = AppVersionType.Minor.DisplayText;
            var newVersion = await execute(input);
            Assert.That(newVersion?.IsMinor(), Is.True, "Should start new minor version");
        }

        [Test]
        public async Task ShouldCreateNewMajorVersion()
        {
            var input = await setup();
            input.Options.Type = AppVersionType.Major.DisplayText;
            var newVersion = await execute(input);
            Assert.That(newVersion?.IsMajor(), Is.True, "Should start new major version");
        }

        [Test]
        public async Task ShouldCreateMilestoneForNewVersion()
        {
            var input = await setup();
            input.Options.Type = AppVersionType.Major.DisplayText;
            var newVersion = await execute(input);
            var milestoneExists = await githubRepo(input).MilestoneExists($"xti_major_version_{newVersion.ID}");
            Assert.That(milestoneExists, Is.True, "Should create milestone for new version");
        }

        [Test]
        public async Task ShouldCreateBranchForNewVersion()
        {
            var input = await setup();
            input.Options.Type = AppVersionType.Major.DisplayText;
            var newVersion = await execute(input);
            var branchExists = await githubRepo(input).BranchExists($"xti/major/{newVersion.ID}");
            Assert.That(branchExists, Is.True, "Should create branch for new version");
        }

        private GitHubXtiRepoClient githubRepo(TestInput input)
        {
            return input.GithubClient.Repo(input.Options.RepoOwner, input.Options.RepoName);
        }

        private async Task<TestInput> setup()
        {
            var hostEnv = new FakeHostEnvironment { EnvironmentName = "Test" };
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", hostEnv.EnvironmentName);
            var services = new ServiceCollection();
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.UseXtiConfiguration("Test", new string[] { });
            var configuration = configurationBuilder.Build();
            services.Configure<GitHubOptions>(configuration.GetSection(GitHubOptions.GitHub));
            services.AddScoped<IHostEnvironment>(sp => hostEnv);
            services.AddWebAppServices(configuration);
            services.AddScoped<GitHubXtiClient, OctoGithubXtiClient>();
            services.AddScoped((sp =>
            {
                var factory = sp.GetService<AppFactory>();
                var clock = sp.GetService<Clock>();
                var githubClient = sp.GetService<GitHubXtiClient>();
                return new NewVersionCommand(factory, clock, githubClient);
            }));
            services.AddScoped<AppDbReset>();
            var sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            var appDbReset = sp.GetService<AppDbReset>();
            await appDbReset.Run();
            await new AppSetup(factory).Run();
            var app = await factory.AppRepository().AddApp(new AppKey("Fake"), DateTime.UtcNow);
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
                Options = new NewVersionOptions
                {
                    App = app.Key().Value,
                    Type = AppVersionType.Patch.DisplayText,
                    RepoOwner = "JasonBenfield",
                    RepoName = "XTI_WebApp"
                };
                GithubClient = sp.GetService<GitHubXtiClient>();
            }

            public AppFactory Factory { get; }
            public App App { get; }
            public NewVersionOptions Options { get; }
            public GitHubXtiClient GithubClient { get; }

            public NewVersionCommand Command() => sp.GetService<NewVersionCommand>();
        }
    }
}
