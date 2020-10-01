using System.Threading.Tasks;

namespace XTI_App.Api
{
    public interface ISessionContext
    {
        Task<IAppSession> Session();
        Task<IAppUser> User();
    }
}
