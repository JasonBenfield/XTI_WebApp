using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class App
    {
        private readonly AppFactory factory;
        private readonly AppRecord record;

        internal App(AppFactory factory, AppRecord record)
        {
            this.factory = factory;
            this.record = record ?? new AppRecord();
        }

        public int ID { get => record.ID; }
        public AppKey Key() => new AppKey(record.Key);

        public Task<AppVersion> StartNewPatch(DateTime timeAdded) =>
            startNewVersion(timeAdded, AppVersionType.Patch);

        public Task<AppVersion> StartNewMinorVersion(DateTime timeAdded) =>
            startNewVersion(timeAdded, AppVersionType.Minor);

        public Task<AppVersion> StartNewMajorVersion(DateTime timeAdded) =>
            startNewVersion(timeAdded, AppVersionType.Major);

        private Task<AppVersion> startNewVersion(DateTime timeAdded, AppVersionType type)
        {
            return factory.VersionRepository().StartNewVersion(this, timeAdded, type);
        }

        public Task<AppVersion> CurrentVersion() =>
            factory.VersionRepository().CurrentVersion(ID);

        public async Task<AppVersion> Version(int id) => (await Versions()).First(v => v.ID == id);

        public Task<IEnumerable<AppVersion>> Versions()
        {
            return factory.VersionRepository().VersionsByApp(this);
        }

        public override string ToString() => $"{nameof(App)} {ID}: {record.Key}";

    }
}
