using System.Collections.Generic;
using System.Threading.Tasks;

namespace XTI_App
{
    public interface IAppUser
    {
        int ID { get; }
        Task<IEnumerable<IAppUserRole>> RolesForApp(IApp app);
    }
}
