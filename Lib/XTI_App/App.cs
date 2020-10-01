using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class App : IApp
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
        public bool Exists() => ID > 0;

        public Task<AppRole> AddRole(AppRoleName name) =>
            factory.RoleRepository().Add(this, name);

        async Task<IEnumerable<IAppRole>> IApp.Roles() =>
            await factory.RoleRepository().RolesForApp(this);

        public Task<IEnumerable<AppRole>> Roles() =>
            factory.RoleRepository().RolesForApp(this);

        public Task<AppRole> Role(AppRoleName roleName) =>
            factory.RoleRepository().Role(this, roleName);

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
            factory.VersionRepository().CurrentVersion(this);

        async Task<IAppVersion> IApp.CurrentVersion() =>
            await factory.VersionRepository().CurrentVersion(this);

        public async Task<AppVersion> Version(int id) =>
            (await Versions()).First(v => v.ID == id);

        async Task<IAppVersion> IApp.Version(int id) =>
            (await Versions()).First(v => v.ID == id);

        public Task<IEnumerable<AppVersion>> Versions() =>
            factory.VersionRepository().VersionsByApp(this);

        public override string ToString() => $"{nameof(App)} {ID}: {record.Key}";
    }
}
