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
            services.Configure<DbOptions>(configuration.GetSection(DbOptions.DB));
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var appDbOptions = sp.GetService<IOptions<DbOptions>>().Value;
                var hostEnvironment = sp.GetService<IHostEnvironment>();
                options.UseSqlServer(new AppConnectionString(appDbOptions, hostEnvironment.EnvironmentName).Value());
                if (hostEnvironment.IsDevOrTest())
                {
                    options.EnableSensitiveDataLogging();
                }
                else
                {
                    options.EnableSensitiveDataLogging(false);
                }
            });
            services.AddScoped<AppFactory, EfAppFactory>();
            services.AddScoped<Clock, UtcClock>();
        }
    }
}
