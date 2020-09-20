using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace XTI_ConsoleApp.Extensions
{
    public static class ConsoleAppExtensions
    {
        public static void ConfigureServicesForConsoleApp(this IServiceCollection services, IConfiguration configuration, Assembly assembly, string[] args)
        {
            services.Configure<ConsoleAppOptions>(configuration.GetSection(ConsoleAppOptions.ConsoleApp));
        }
    }
}
