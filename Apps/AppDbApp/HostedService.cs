﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_App.EF;
using XTI_ConsoleApp.Extensions;

namespace AppDbApp
{
    public sealed class HostedService : IHostedService
    {
        private readonly IHostApplicationLifetime lifetime;
        private readonly IHostEnvironment hostEnvironment;
        private readonly AppDbReset appDbReset;
        private readonly AppDbAppOptions options;

        public HostedService(IHostApplicationLifetime lifetime, IHostEnvironment hostEnvironment, AppDbReset appDbReset, IOptions<AppDbAppOptions> options)
        {
            this.lifetime = lifetime;
            this.hostEnvironment = hostEnvironment;
            this.appDbReset = appDbReset;
            this.options = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (options.Command == "reset")
                {
                    if (!hostEnvironment.IsTest())
                    {
                        throw new ArgumentException("Database reset can only be run for the test environment");
                    }
                    await appDbReset.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
