using System.Threading.Tasks;

namespace XTI_App.Api
{
    public sealed class AppApiSuperUser : IAppApiUser
    {
        public Task<bool> HasAccess(ResourceAccess resourceAccess, AccessModifier modifier)
        {
            return Task.FromResult(true);
        }
    }
}
