using System;
using System.Threading.Tasks;

namespace XTI_App
{
    public interface IAppSession
    {
        int ID { get; }
        bool HasStarted();
        bool HasEnded();
        Task<AppRequest> LogRequest(IAppVersion version, string path, DateTime timeRequested);
        Task Authenticate(IAppUser user);
        Task End(DateTime timeEnded);
    }
}
