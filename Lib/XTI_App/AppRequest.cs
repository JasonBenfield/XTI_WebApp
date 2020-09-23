using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppRequest
    {
        internal AppRequest(AppFactory factory, AppRequestRecord record)
        {
            this.factory = factory;
            this.record = record ?? new AppRequestRecord();
        }

        private readonly AppFactory factory;
        private readonly AppRequestRecord record;

        public int ID { get => record.ID; }
        public XtiPath ResourceName() => XtiPath.Parse(record.Path);

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
                this, timeOccurred, AppEventSeverity.CriticalError, caption, ex.Message, ex.StackTrace
            );
        }

        public override string ToString() => $"{nameof(AppRequest)} {ID}";

    }
}
