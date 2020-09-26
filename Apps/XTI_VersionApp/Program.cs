using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.EF;
using XTI_Configuration.Extensions;
using XTI_ConsoleApp.Extensions;
using XTI_Version;
using XTI_Version.Octo;

namespace XTI_VersionApp
{
    class Program
    {
        static Task Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.UseXtiConfiguration(hostingContext.HostingEnvironment.EnvironmentName, args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddConsoleAppServices(hostContext.Configuration);
                    services.Configure<GitHubOptions>(hostContext.Configuration.GetSection(GitHubOptions.GitHub));
                    services.Configure<ManageVersionOptions>(hostContext.Configuration.GetSection(ManageVersionOptions.ManageVersion));
                    services.AddSingleton<GitHubXtiClient, OctoGithubXtiClient>();
                    services.AddSingleton<BeginPublishVersionCommand>();
                    services.AddSingleton<EndPublishVersionCommand>();
                    services.AddSingleton(sp =>
                    {
                        var appFactory = sp.GetService<AppFactory>();
                        var clock = sp.GetService<Clock>();
                        var gitHubClient = sp.GetService<GitHubXtiClient>();
                        return new NewVersionCommand(appFactory, clock, gitHubClient);
                    });
                    services.AddSingleton<ManageVersionCommand>();
                    services.AddHostedService<VersionManager>();
                })
                .RunConsoleAsync();
        }
    }
}
