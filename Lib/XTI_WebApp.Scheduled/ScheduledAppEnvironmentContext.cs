using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using XTI_App;
using XTI_TempLog;

namespace XTI_WebApp.Scheduled
{
    public sealed class ScheduledAppEnvironmentContext : IAppEnvironmentContext
    {
        public Task<AppEnvironment> Value()
        {
            var firstMacAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();
            var env = new AppEnvironment
            (
                AppUserName.SuperUser.Value,
                firstMacAddress,
                Environment.MachineName,
                $"{RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}",
                AppType.Values.WebApp.DisplayText
            );
            return Task.FromResult(env);
        }
    }
}
