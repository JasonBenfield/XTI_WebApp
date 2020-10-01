﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppRoleRepository
    {
        private readonly AppFactory factory;
        private readonly DataRepository<AppRoleRecord> repo;

        internal AppRoleRepository(AppFactory factory, DataRepository<AppRoleRecord> repo)
        {
            this.factory = factory;
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
            return factory.CreateAppRole(record);
        }

        internal async Task<IEnumerable<AppRole>> RolesForApp(App app)
        {
            var records = await repo.Retrieve()
                .Where(r => r.AppID == app.ID)
                .ToArrayAsync();
            return records.Select(r => factory.CreateAppRole(r));
        }

        internal async Task<AppRole> Role(App app, AppRoleName roleName)
        {
            var record = await repo.Retrieve()
                .Where(r => r.AppID == app.ID && r.Name == roleName.Value)
                .FirstOrDefaultAsync();
            return factory.CreateAppRole(record);
        }

        internal IQueryable<int> RoleIDsForApp(IApp app)
        {
            return repo.Retrieve()
                .Where(r => r.AppID == app.ID)
                .Select(r => r.ID);
        }
    }
}
