﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppVersion : IAppVersion
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
        private int Major { get => record.Major; }
        private int Minor { get => record.Minor; }
        private int Patch { get => record.Patch; }

        public AppVersionKey Key() => AppVersionKey.Parse(record.VersionKey);

        public bool IsPublishing() => Status().Equals(AppVersionStatus.Publishing);
        public bool IsCurrent() => Status().Equals(AppVersionStatus.Current);
        public bool IsNew() => Status().Equals(AppVersionStatus.New);

        public bool IsPatch() => Type().Equals(AppVersionType.Patch);
        public bool IsMinor() => Type().Equals(AppVersionType.Minor);
        public bool IsMajor() => Type().Equals(AppVersionType.Major);

        private AppVersionStatus Status() => AppVersionStatus.FromValue(record.Status);
        private AppVersionType Type() => AppVersionType.FromValue(record.Type);

        public Version Version() => new Version(Major, Minor, Patch);
        public Version NextMajor() => new Version(Major + 1, 0, 0);
        public Version NextMinor() => new Version(Major, Minor + 1, 0);
        public Version NextPatch() => new Version(Major, Minor, Patch + 1);

        public async Task Publishing()
        {
            var app = await factory.Apps().App(record.AppID);
            var current = await app.CurrentVersion();
            await repo.Update(record, r =>
            {
                r.Status = AppVersionStatus.Publishing.Value;
                var type = Type();
                Version nextVersion;
                if (type.Equals(AppVersionType.Major))
                {
                    nextVersion = current.NextMajor();
                }
                else if (type.Equals(AppVersionType.Minor))
                {
                    nextVersion = current.NextMinor();
                }
                else if (type.Equals(AppVersionType.Patch))
                {
                    nextVersion = current.NextPatch();
                }
                else
                {
                    throw new NotSupportedException($"Version type '{type}' is not supported");
                }
                r.Major = nextVersion.Major;
                r.Minor = nextVersion.Minor;
                r.Patch = nextVersion.Build;
            });
        }

        public async Task Published()
        {
            var app = await factory.Apps().App(record.AppID);
            var current = await app.CurrentVersion();
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
