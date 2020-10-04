using System.Threading.Tasks;

namespace XTI_WebAppClient
{
    public sealed class XtiToken
    {
        private readonly IAuthClient authClient;
        private readonly XtiCredentials credentials;
        private string token;

        public XtiToken(IAuthClient authClient, XtiCredentials credentials)
        {
            this.authClient = authClient;
            this.credentials = credentials;
        }

        public void Reset() => token = null;

        public async Task<string> Value()
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                var loginModel = new LoginModel
                {
                    UserName = credentials.UserName,
                    Password = credentials.Password
                };
                var result = await authClient.AuthApi.Authenticate(loginModel);
                token = result.Token;
            }
            return token;
        }
    }
}
