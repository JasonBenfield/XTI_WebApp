using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using XTI_App.EF;
using XTI_Configuration.Extensions;
using XTI_WebApp;

namespace EfMigrationsApp
{
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            configBuilder.UseXtiConfiguration(environment, args);
            var config = configBuilder.Build();
            var section = config.GetSection(WebAppOptions.WebApp);
            var webAppOptions = section.Get<WebAppOptions>();
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer
                (
                    webAppOptions.ConnectionString,
                    b => b.MigrationsAssembly("EfMigrationsApp")
                )
                .EnableSensitiveDataLogging();
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
