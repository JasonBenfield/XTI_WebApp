using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppRequest
    {
        internal AppRequest(AppFactory factory, DataRepository<AppRequestRecord> repo, AppRequestRecord record)
        {
            this.repo = repo;
            this.factory = factory;
            this.record = record ?? new AppRequestRecord();
        }

        private readonly AppFactory factory;
        private readonly DataRepository<AppRequestRecord> repo;
        private readonly AppRequestRecord record;

        public int ID { get => record.ID; }
        public XtiPath ResourceName() => XtiPath.Parse(record.Path);
        public bool HasEnded() => new Timestamp(record.TimeEnded).IsValid();

        public Task<AppVersion> Version()
        {
            return factory.VersionRepository().Version(record.VersionID);
        }

        public Task<IEnumerable<AppEvent>> Events()
        {
            var eventRepo = factory.EventRepository();
            return eventRepo.RetrieveByRequest(this);
        }

        public Task LogCriticalException(DateTime timeOccurred, Exception ex, string caption)
        {
            var eventRepo = factory.EventRepository();
            return eventRepo.LogEvent
            (
                this, timeOccurred, AppEventSeverity.Values.CriticalError, caption, ex.Message, ex.StackTrace
            );
        }

        public Task End(DateTime timeEnded)
        {
            return repo.Update(record, r =>
            {
                r.TimeEnded = timeEnded;
            });
        }

        public override string ToString() => $"{nameof(AppRequest)} {ID}";

    }
}
