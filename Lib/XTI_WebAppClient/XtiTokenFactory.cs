using XTI_Credentials;

namespace XTI_WebAppClient
{
    public sealed class XtiTokenFactory : IXtiTokenFactory
    {
        private readonly ICredentials credentials;

        public XtiTokenFactory(ICredentials credentials)
        {
            this.credentials = credentials;
        }

        public IXtiToken Create(IAuthClient authClient)
            => new XtiToken(authClient, credentials);
    }
}
