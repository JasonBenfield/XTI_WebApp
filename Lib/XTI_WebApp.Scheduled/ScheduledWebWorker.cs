using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_Schedule;
using XTI_App.Hosting;

namespace XTI_WebApp.Scheduled
{
    public sealed class ScheduledWebWorker : BackgroundService
    {
        private readonly IServiceProvider sp;
        private readonly AppActionOptions options;

        public ScheduledWebWorker(IServiceProvider sp, IOptions<AppActionOptions> options)
        {
            this.sp = sp;
            this.options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var basePath = sp.GetService<XtiBasePath>();
            if (basePath.Value().IsCurrentVersion())
            {
                var multiActionRunner = new MultiActionRunner
                (
                    sp,
                    options.ImmediateActions,
                    options.ScheduledActions,
                    options.AlwaysRunningActions
                );
                await multiActionRunner.Start(stoppingToken);
            }
        }
    }
}
