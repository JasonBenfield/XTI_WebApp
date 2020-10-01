using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppRoleRepository
    {
        private readonly DataRepository<AppRoleRecord> repo;

        internal AppRoleRepository(DataRepository<AppRoleRecord> repo)
        {
            this.repo = repo;
        }

        internal async Task<AppRole> Add(App app, AppRoleName name)
        {
            var record = new AppRoleRecord
            {
                AppID = app.ID,
                Name = name.Value
            };
            await repo.Create(record);
            return new AppRole(record);
        }

        internal async Task<IEnumerable<AppRole>> RolesForApp(App app)
        {
            var records = await repo.Retrieve()
                .Where(r => r.AppID == app.ID)
                .ToArrayAsync();
            return records.Select(r => new AppRole(r));
        }
    }
}
