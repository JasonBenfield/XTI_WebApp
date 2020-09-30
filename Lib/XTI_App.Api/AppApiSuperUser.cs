using System.Threading.Tasks;

namespace XTI_App.Api
{
    public sealed class AppApiSuperUser : AppApiUser
    {
        public Task<bool> HasAccess(ResourceAccess resourceAccess)
        {
            return Task.FromResult(true);
        }
    }
}
