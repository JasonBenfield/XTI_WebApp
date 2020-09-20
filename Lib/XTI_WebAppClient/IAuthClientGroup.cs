using System.Threading.Tasks;

namespace XTI_WebAppClient
{
    public interface IAuthClientGroup
    {
        public Task<LoginResult> Authenticate(LoginModel model);
    }
}
