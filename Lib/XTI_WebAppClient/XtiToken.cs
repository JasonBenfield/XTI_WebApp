using System.Threading.Tasks;
using XTI_Credentials;

namespace XTI_WebAppClient
{
    public sealed class XtiToken
    {
        private readonly IAuthClient authClient;
        private readonly ICredentials credentials;
        private string token;

        public XtiToken(IAuthClient authClient, ICredentials credentials)
        {
            this.authClient = authClient;
            this.credentials = credentials;
        }

        public void Reset() => token = null;

        public async Task<string> Value()
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                var value = await credentials.Value();
                var loginModel = new LoginModel
                {
                    UserName = value.UserName,
                    Password = value.Password
                };
                var result = await authClient.AuthApi.Authenticate(loginModel);
                token = result.Token;
            }
            return token;
        }
    }
}
