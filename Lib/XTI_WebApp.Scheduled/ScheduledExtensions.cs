using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XTI_App.Hosting;
using XTI_Schedule;

namespace XTI_WebApp.Scheduled
{
    public static class ScheduledExtensions
    {
        public static void AddScheduledWebServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AppActionOptions>(config.GetSection(AppActionOptions.AppAction));
            services.AddScoped<IActionRunnerFactory, WebActionRunnerFactory>();
            services.AddSingleton<ScheduledAppEnvironmentContext>();
            services.AddSingleton<XtiBasePath>();
            services.AddHostedService<ScheduledWebWorker>();
        }
    }
}
