using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppVersion
    {
        private readonly AppFactory factory;
        private readonly DataRepository<AppVersionRecord> repo;
        private readonly AppVersionRecord record;

        internal AppVersion(AppFactory factory, DataRepository<AppVersionRecord> repo, AppVersionRecord record)
        {
            this.factory = factory;
            this.repo = repo;
            this.record = record ?? new AppVersionRecord();
        }

        public int ID { get => record.ID; }
        public int Major { get => record.Major; }
        public int Minor { get => record.Minor; }
        public int Patch { get => record.Patch; }

        public bool IsPublishing() => Status().Equals(AppVersionStatus.Publishing);
        public bool IsCurrent() => Status().Equals(AppVersionStatus.Current);
        public bool IsNew() => Status().Equals(AppVersionStatus.New);

        public bool IsPatch() => Type().Equals(AppVersionType.Patch);
        public bool IsMinor() => Type().Equals(AppVersionType.Minor);
        public bool IsMajor() => Type().Equals(AppVersionType.Major);

        private AppVersionStatus Status() => AppVersionStatus.FromValue(record.Status);
        private AppVersionType Type() => AppVersionType.FromValue(record.Type);

        public async Task Publishing()
        {
            var current = await factory.VersionRepository().CurrentVersion(record.AppID);
            await repo.Update(record, r =>
            {
                r.Status = AppVersionStatus.Publishing.Value;
                var type = Type();
                if (type.Equals(AppVersionType.Major))
                {
                    r.Major = current.Major + 1;
                }
                else if (type.Equals(AppVersionType.Minor))
                {
                    r.Major = current.Major;
                    r.Minor = current.Minor + 1;
                }
                else if (type.Equals(AppVersionType.Patch))
                {
                    r.Major = current.Major;
                    r.Minor = current.Minor;
                    r.Patch = current.Patch + 1;
                }
            });
        }

        public async Task Published()
        {
            var current = await factory.VersionRepository().CurrentVersion(record.AppID);
            if (current.IsCurrent())
            {
                await current.Archive();
            }
            await repo.Update(record, r =>
            {
                r.Status = AppVersionStatus.Current.Value;
            });
        }

        private Task Archive()
        {
            return repo.Update(record, r =>
            {
                r.Status = AppVersionStatus.Old.Value;
            });
        }

        public override string ToString() => $"{nameof(AppVersion)} {ID}: {Major}.{Minor}.{Patch}";

    }
}
