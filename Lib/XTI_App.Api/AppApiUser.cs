using System.Threading.Tasks;

namespace XTI_App.Api
{
    public interface AppApiUser
    {
        Task<bool> HasAccess(ResourceAccess resourceAccess);
    }
}
