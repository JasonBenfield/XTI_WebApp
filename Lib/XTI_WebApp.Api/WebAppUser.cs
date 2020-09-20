using System.Threading.Tasks;

namespace XTI_WebApp.Api
{
    public interface WebAppUser
    {
        Task<bool> HasAccess(ResourceAccess resourceAccess);
    }
}
