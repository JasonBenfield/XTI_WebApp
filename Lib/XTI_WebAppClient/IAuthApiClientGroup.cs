using System.Threading.Tasks;

namespace XTI_WebAppClient
{
    public interface IAuthApiClientGroup
    {
        public Task<LoginResult> Authenticate(LoginCredentials model);
    }
}
