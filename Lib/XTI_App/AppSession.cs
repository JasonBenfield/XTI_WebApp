using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppSession
    {
        private readonly AppFactory factory;
        private readonly DataRepository<AppSessionRecord> repo;
        private readonly AppSessionRecord record;

        internal AppSession(AppFactory factory, DataRepository<AppSessionRecord> repo, AppSessionRecord record)
        {
            this.factory = factory;
            this.repo = repo;
            this.record = record ?? new AppSessionRecord();
        }

        public int ID { get => record.ID; }
        public bool IsNotFound() => ID <= 0;

        public bool HasEnded() => !new Timestamp(record.TimeEnded).IsEmpty();

        public Task<AppRequest> LogRequest(AppVersion version, string path, DateTime timeRequested)
        {
            var requestRepo = factory.RequestRepository();
            return requestRepo.Add(this, version, path, timeRequested);
        }

        public Task Authenticate(AppUser user)
        {
            return repo.Update(record, r =>
            {
                r.UserID = user.ID;
            });
        }

        public Task End(DateTime timeEnded)
        {
            return repo.Update(record, r =>
            {
                r.TimeEnded = timeEnded;
            });
        }

        public Task<IEnumerable<AppRequest>> Requests()
        {
            var requestRepo = factory.RequestRepository();
            return requestRepo.RetrieveBySession(this);
        }

        public override string ToString() => $"{nameof(AppSession)} {ID}";
    }
}
