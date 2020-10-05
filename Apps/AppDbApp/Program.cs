﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using XTI_App.EF;
using XTI_Configuration.Extensions;
using XTI_ConsoleApp.Extensions;

namespace AppDbApp
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
                    services.Configure<AppDbAppOptions>(hostContext.Configuration);
                    services.AddConsoleAppServices(hostContext.Configuration);
                    services.AddScoped<AppDbReset>();
                    services.AddScoped<AppDbBackup>();
                    services.AddScoped<AppDbRestore>();
                    services.AddHostedService(sp =>
                    {
                        var scope = sp.CreateScope();
                        var lifetime = scope.ServiceProvider.GetService<IHostApplicationLifetime>();
                        var appDbReset = scope.ServiceProvider.GetService<AppDbReset>();
                        var appDbBackup = scope.ServiceProvider.GetService<AppDbBackup>();
                        var appDbRestore = scope.ServiceProvider.GetService<AppDbRestore>();
                        var options = scope.ServiceProvider.GetService<IOptions<AppDbAppOptions>>();
                        return new HostedService
                        (
                            lifetime,
                            hostContext.HostingEnvironment,
                            appDbReset,
                            appDbBackup,
                            appDbRestore,
                            options
                        );
                    });
                })
                .RunConsoleAsync();
        }
    }
}
