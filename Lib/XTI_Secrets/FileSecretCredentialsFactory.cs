using Microsoft.AspNetCore.DataProtection;

namespace XTI_Secrets
{
    public sealed class FileSecretCredentialsFactory : SecretCredentialsFactory
    {
        public FileSecretCredentialsFactory(IDataProtector dataProtector) : base(dataProtector)
        {
        }

        protected override SecretCredentials _Create(string key, IDataProtector dataProtector) =>
            new FileSecretCredentials(key, dataProtector);
    }
}
