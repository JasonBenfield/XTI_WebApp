using System.Threading.Tasks;
using XTI_Credentials;

namespace XTI_WebAppClient
{
    public sealed class XtiToken : IXtiToken
    {
        private readonly IAuthClient authClient;
        private readonly ICredentials credentials;

        public XtiToken(IAuthClient authClient, ICredentials credentials)
        {
            this.authClient = authClient;
            this.credentials = credentials;
        }

        public void Reset() { }

        public async Task<string> Value()
        {
            var value = await credentials.Value();
            var loginModel = new LoginCredentials
            {
                UserName = value.UserName,
                Password = value.Password
            };
            var result = await authClient.AuthApi.Authenticate(loginModel);
            return result.Token;
        }
    }
}
