using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using XTI_App;
using XTI_App.EF;
using Microsoft.EntityFrameworkCore;

namespace XTI_ConsoleApp.Extensions
{
    public static class ConsoleAppExtensions
    {
        public static bool IsDevOrTest(this IHostEnvironment env) => env.IsDevelopment() || env.IsEnvironment("Test");

        public static void AddConsoleAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConsoleAppOptions>(configuration.GetSection(ConsoleAppOptions.ConsoleApp));
            services.Configure<AppDbOptions>(configuration.GetSection(AppDbOptions.AppDb));
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var appDbOptions = sp.GetService<IOptions<AppDbOptions>>().Value;
                options.UseSqlServer(appDbOptions.ConnectionString);
                var hostEnvironment = sp.GetService<IHostEnvironment>();
                if (hostEnvironment.IsDevOrTest())
                {
                    options.EnableSensitiveDataLogging();
                }
                else
                {
                    options.EnableSensitiveDataLogging(false);
                }
            });
            services.AddSingleton<AppFactory, EfAppFactory>();
            services.AddSingleton<Clock, UtcClock>();
        }
    }
}
