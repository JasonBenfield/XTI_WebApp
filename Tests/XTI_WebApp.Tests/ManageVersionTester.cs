using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_Version;
using XTI_Version.Fakes;
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
        public FakeGithubXtiClient GithubClient { get; private set; }

        public async Task Setup()
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
            services.AddSingleton(sp =>
            {
                var factory = sp.GetService<AppFactory>();
                return new BeginPublishVersionCommand(factory);
            });
            services.AddSingleton(sp =>
            {
                var factory = sp.GetService<AppFactory>();
                return new EndPublishVersionCommand(factory);
            });
            services.AddSingleton(sp =>
            {
                var newVersionCommand = sp.GetService<NewVersionCommand>();
                var beginPublishVersionCommand = sp.GetService<BeginPublishVersionCommand>();
                var endPublishVersionCommand = sp.GetService<EndPublishVersionCommand>();
                return new ManageVersionCommand(newVersionCommand, beginPublishVersionCommand, endPublishVersionCommand);
            });
            sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            var setup = new AppSetup(factory);
            await setup.Run();
            var app = await factory.Apps().AddApp(new AppKey("Fake"), "Fake", DateTime.UtcNow);
            Factory = sp.GetService<AppFactory>();
            App = app;
            Clock = sp.GetService<FakeClock>();
            Options = new ManageVersionOptions
            {
                Command = "New",
                PublishVersion = new PublishVersionOptions
                {
                    Branch = ""
                },
                NewVersion = new NewVersionOptions
                {
                    App = app.Key().Value,
                    Type = AppVersionType.Patch.DisplayText,
                    RepoOwner = "https://github.com/JasonBenfield/FakeWebApp"
                }
            };
            GithubClient = (FakeGithubXtiClient)sp.GetService<GitHubXtiClient>();
        }

        public Task<AppVersion> Execute()
        {
            var command = Command();
            return command.Execute(Options);
        }

        public ManageVersionCommand Command() => sp.GetService<ManageVersionCommand>();
    }
}
