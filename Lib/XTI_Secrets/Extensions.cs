using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace XTI_Secrets
{
    public static class Extensions
    {
        public static void AddFileSecretCredentials(this IServiceCollection services)
        {
            services.AddScoped<SecretCredentialsFactory>(sp =>
            {
                var dataProtector = sp.GetDataProtector(new[] { "XTI_Secrets" });
                return new FileSecretCredentialsFactory(dataProtector);
            });
        }
    }
}
