﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppVersionRepository
    {
        private readonly AppFactory factory;
        private readonly DataRepository<AppVersionRecord> repo;

        internal AppVersionRepository(AppFactory factory, DataRepository<AppVersionRecord> repo)
        {
            this.factory = factory;
            this.repo = repo;
        }

        internal async Task<AppVersion> StartNewVersion(App app, DateTime timeAdded, AppVersionType type)
        {
            var record = new AppVersionRecord
            {
                AppID = app.ID,
                Major = 0,
                Minor = 0,
                Patch = 0,
                TimeAdded = timeAdded,
                Description = "",
                Status = AppVersionStatus.New.Value,
                Type = type.Value
            };
            await repo.Create(record);
            return factory.CreateVersion(record);
        }

        public async Task<AppVersion> Version(int versionID)
        {
            var record = await repo.Retrieve().FirstOrDefaultAsync(v => v.ID == versionID);
            return factory.CreateVersion(record);
        }

        internal async Task<IEnumerable<AppVersion>> VersionsByApp(App app)
        {
            var records = await repo.Retrieve().Where(v => v.AppID == app.ID).ToArrayAsync();
            return records.Select(v => factory.CreateVersion(v));
        }

        internal async Task<AppVersion> CurrentVersion(App app)
        {
            var record = await repo.Retrieve()
                .Where(v => v.AppID == app.ID && v.Status == AppVersionStatus.Current.Value)
                .FirstOrDefaultAsync();
            return factory.CreateVersion(record);
        }
    }
}
