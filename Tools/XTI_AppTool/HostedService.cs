﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XTI_App;

namespace XTI_AppTool
{
    public sealed class HostedService : IHostedService
    {
        private readonly IHostApplicationLifetime lifetime;
        private readonly AppFactory appFactory;
        private readonly Clock clock;
        private readonly AppToolOptions appToolOptions;

        public HostedService
        (
            IHostApplicationLifetime lifetime,
            AppFactory appFactory,
            Clock clock,
            IOptions<AppToolOptions> appToolOptions
        )
        {
            this.lifetime = lifetime;
            this.appFactory = appFactory;
            this.clock = clock;
            this.appToolOptions = appToolOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(appToolOptions.AppKey))
                {
                    throw new ArgumentException("App key is required");
                }
                if (string.IsNullOrWhiteSpace(appToolOptions.AppType))
                {
                    throw new ArgumentException("App type is required");
                }
                var appType = AppType.Values.Value(appToolOptions.AppType);
                if (appType.Equals(AppType.Values.NotFound))
                {
                    throw new ArgumentException($"App type '{appToolOptions.AppType}' is not valid");
                }
                var appKey = new AppKey(appToolOptions.AppKey);
                var app = await appFactory.Apps().App(appKey, appType);
                if (appToolOptions.Command == "add")
                {
                    var title = string.IsNullOrWhiteSpace(appToolOptions.AppTitle)
                        ? appToolOptions.AppKey
                        : appToolOptions.AppTitle;
                    if (app.Exists())
                    {
                        await app.SetTitle(title);
                    }
                    else
                    {
                        await appFactory.Apps().AddApp(appKey, appType, title, clock.Now());
                    }
                    var currentVersion = await app.CurrentVersion();
                    if (!currentVersion.IsCurrent())
                    {
                        var version = await app.StartNewMajorVersion(clock.Now());
                        await version.Publishing();
                        await version.Published();
                    }
                }
                else if (appToolOptions.Command == "get-dev-version")
                {
                    var currentVersion = await app.CurrentVersion();
                    var nextPatch = currentVersion.NextPatch();
                    var devVersion = $"{nextPatch.Major}.{nextPatch.Minor}.{nextPatch.Build}-dev{DateTime.Now:yyMMdd_HHmmssfff}";
                    using var writer = new StreamWriter("dev_version.txt", false);
                    writer.WriteLine(devVersion);
                }
                else
                {
                    throw new NotSupportedException($"Command '{appToolOptions.Command}' is not supported");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.ExitCode = 999;
            }
            lifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
