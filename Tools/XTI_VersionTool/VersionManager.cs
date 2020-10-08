using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using XTI_Version;

namespace XTI_VersionApp
{
    public sealed class VersionManager : IHostedService
    {
        private readonly IHostApplicationLifetime lifetime;
        private readonly ManageVersionCommand manageVersionCommand;
        private readonly ManageVersionOptions manageVersionOptions;

        public VersionManager
        (
            IHostApplicationLifetime lifetime,
            ManageVersionCommand manageVersionCommand,
            IOptions<ManageVersionOptions> manageVersionOptions
        )
        {
            this.lifetime = lifetime;
            this.manageVersionCommand = manageVersionCommand;
            this.manageVersionOptions = manageVersionOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await manageVersionCommand.Execute(manageVersionOptions);
            lifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
