using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace XTI_Secrets
{
    public static class Extensions
    {
        public static void AddFileSecretCredentials(this IServiceCollection services)
        {
            services.AddScoped<SecretCredentialsFactory>(sp =>
            {
                var hostEnv = sp.GetService<IHostEnvironment>();
                var dataProtector = sp.GetDataProtector(new[] { "XTI_Secrets" });
                return new FileSecretCredentialsFactory(hostEnv.EnvironmentName, dataProtector);
            });
        }
    }
}
