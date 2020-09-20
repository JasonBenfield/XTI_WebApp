using System.Threading.Tasks;

namespace XTI_WebApp.Api
{
    public sealed class SuperUser : WebAppUser
    {
        public Task<bool> HasAccess(ResourceAccess resourceAccess)
        {
            return Task.FromResult(true);
        }
    }
}
