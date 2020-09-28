﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppSessionRepository
    {
        private readonly AppFactory factory;
        private readonly DataRepository<AppSessionRecord> repo;

        internal AppSessionRepository(AppFactory factory, DataRepository<AppSessionRecord> repo)
        {
            this.factory = factory;
            this.repo = repo;
        }

        public async Task<AppSession> RetrieveByID(int id)
        {
            var record = await repo.Retrieve().FirstOrDefaultAsync(s => s.ID == id);
            return factory.CreateAppSession(record);
        }

        public async Task<IEnumerable<AppSession>> RetrieveByDateRange(DateTime startDate, DateTime endDate)
        {
            var records = await repo.Retrieve()
                .Where(s => s.TimeStarted >= startDate.Date && s.TimeStarted < endDate.Date.AddDays(1))
                .ToArrayAsync();
            return records.Select(s => factory.CreateAppSession(s));
        }

        public async Task<AppSession> Create(AppUser user, DateTime timeStarted, string userAgent, string remoteAddress)
        {
            var record = new AppSessionRecord
            {
                UserID = user.ID,
                TimeStarted = timeStarted,
                TimeEnded = Timestamp.MaxValue.Value,
                UserAgent = userAgent,
                RemoteAddress = remoteAddress
            };
            await repo.Create(record);
            return factory.CreateAppSession(record);
        }
    }
}
