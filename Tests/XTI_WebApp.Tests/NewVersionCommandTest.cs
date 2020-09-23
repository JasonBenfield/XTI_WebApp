using FakeWebApp.Api;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XTI_App;
using XTI_Version;
using XTI_Version.Fakes;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.Tests
{
    public sealed class NewVersionCommandTest
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
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            services.AddSingleton<GitHubXtiClient, FakeGithubXtiClient>();
            services.AddSingleton(sp =>
            {
                var factory = sp.GetService<AppFactory>();
                var clock = sp.GetService<Clock>();
                var githubClient = sp.GetService<GitHubXtiClient>();
                return new NewVersionCommand(factory, clock, githubClient);
            });
            var sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            var setup = new AppSetup(factory);
            await setup.Run();
            await new FakeAppSetup(sp).Run();
            var app = await factory.AppRepository().App(FakeAppApi.AppKey);
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
            private readonly IServiceProvider sp;

            public TestInput(IServiceProvider sp, App app)
            {
                this.sp = sp;
                Factory = sp.GetService<AppFactory>();
                App = app;
                Clock = sp.GetService<FakeClock>();
                Options = new NewVersionOptions
                {
                    App = app.Key().Value,
                    Type = AppVersionType.Patch.DisplayText,
                    RepoOwner = "https://github.com/JasonBenfield/FakeWebApp"
                };
                GithubClient = (FakeGithubXtiClient)sp.GetService<GitHubXtiClient>();
            }

            public AppFactory Factory { get; }
            public App App { get; }
            public FakeClock Clock { get; }
            public NewVersionOptions Options { get; }
            public FakeGithubXtiClient GithubClient { get; }

            public NewVersionCommand Command() => sp.GetService<NewVersionCommand>();
        }
    }
}
