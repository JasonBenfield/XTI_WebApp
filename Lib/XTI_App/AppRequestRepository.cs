using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppRequestRepository
    {
        private readonly AppFactory factory;
        private readonly DataRepository<AppRequestRecord> repo;

        internal AppRequestRepository(AppFactory factory, DataRepository<AppRequestRecord> repo)
        {
            this.factory = factory;
            this.repo = repo;
        }

        internal async Task<AppRequest> Add(IAppSession session, IAppVersion version, string path, DateTime timeRequested)
        {
            var record = new AppRequestRecord
            {
                SessionID = session.ID,
                VersionID = version.ID,
                Path = path,
                TimeStarted = timeRequested
            };
            await repo.Create(record);
            return factory.CreateAppRequest(record);
        }

        internal async Task<IEnumerable<AppRequest>> RetrieveBySession(AppSession session)
        {
            var requests = await repo.Retrieve()
                .Where(r => r.SessionID == session.ID)
                .ToArrayAsync();
            return requests.Select(r => factory.CreateAppRequest(r));
        }
    }
}
