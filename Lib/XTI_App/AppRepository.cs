﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppRepository
    {
        private readonly AppFactory factory;
        private readonly DataRepository<AppRecord> repo;

        internal AppRepository(AppFactory factory, DataRepository<AppRecord> repo)
        {
            this.factory = factory;
            this.repo = repo;
        }

        public async Task<App> AddApp(string key, DateTime timeAdded)
        {
            var record = new AppRecord
            {
                Key = key,
                TimeAdded = timeAdded
            };
            await repo.Create(record);
            return factory.CreateApp(record);
        }

        public async Task<App> App(int id)
        {
            var record = await repo.Retrieve().FirstOrDefaultAsync(a => a.ID == id);
            return factory.CreateApp(record);
        }

        public async Task<App> App(AppKey key)
        {
            var record = await repo.Retrieve().FirstOrDefaultAsync(a => key.Equals(a.Key));
            return factory.CreateApp(record);
        }
    }
}
