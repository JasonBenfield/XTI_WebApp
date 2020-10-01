using System.Collections.Generic;
using System.Threading.Tasks;

namespace XTI_App
{
    public interface IApp
    {
        int ID { get; }
        Task<IAppVersion> CurrentVersion();
        Task<IAppVersion> Version(int id);
        Task<IEnumerable<IAppRole>> Roles();
    }
}
