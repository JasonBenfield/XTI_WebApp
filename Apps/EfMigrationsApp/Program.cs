using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using XTI_WebApp;
using XTI_Configuration.Extensions;

namespace EfMigrationsApp
{
    class Program
    {
        static Task Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            configBuilder.UseXtiConfiguration(environment, args);
            var config = configBuilder.Build();
            var section = config.GetSection(WebAppOptions.WebApp);
            var webAppOptions = section.Get<WebAppOptions>();
            Console.WriteLine($"environment: {environment}\r\nconnectionString: {webAppOptions.ConnectionString}");
            Console.ReadLine();
            return Task.CompletedTask;
        }
    }
}
