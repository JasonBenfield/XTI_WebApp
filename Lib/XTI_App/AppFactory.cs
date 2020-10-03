﻿using System;

namespace XTI_App
{
    public abstract class AppFactory
    {
        protected AppFactory() { }

        private AppUserRepository userRepo;
        public AppUserRepository UserRepository() =>
            fetchRepo<AppUserRecord, AppUserRepository>(ref userRepo,
                dataRepo => new AppUserRepository(this, dataRepo));
        internal AppUser CreateAppUser(AppUserRecord record) => new AppUser(this, record);

        private AppSessionRepository sessionRepo;
        public AppSessionRepository SessionRepository() =>
            fetchRepo<AppSessionRecord, AppSessionRepository>(ref sessionRepo,
                dataRepo => new AppSessionRepository(this, dataRepo));
        internal AppSession CreateAppSession(AppSessionRecord record) =>
            new AppSession(this, CreateDataRepository<AppSessionRecord>(), record);

        private AppRequestRepository requestRepo;
        public AppRequestRepository RequestRepository() =>
            fetchRepo<AppRequestRecord, AppRequestRepository>(ref requestRepo,
                dataRepo => new AppRequestRepository(this, dataRepo));
        internal AppRequest CreateAppRequest(AppRequestRecord record) =>
            new AppRequest(this, CreateDataRepository<AppRequestRecord>(), record);

        private AppEventRepository eventRepo;
        public AppEventRepository EventRepository() =>
            fetchRepo<AppEventRecord, AppEventRepository>(ref eventRepo,
                dataRepo => new AppEventRepository(this, dataRepo));
        internal AppEvent CreateEvent(AppEventRecord record) => new AppEvent(record);

        private AppRepository appRepo;
        public AppRepository AppRepository() =>
            fetchRepo<AppRecord, AppRepository>(ref appRepo, dataRepo =>
                new AppRepository(this, dataRepo));
        internal App CreateApp(AppRecord record) =>
            new App(CreateDataRepository<AppRecord>(), this, record);

        private AppVersionRepository versionRepo;
        public AppVersionRepository VersionRepository() =>
            fetchRepo<AppVersionRecord, AppVersionRepository>(ref versionRepo, dataRepo =>
                new AppVersionRepository(this, dataRepo));
        internal AppVersion CreateVersion(AppVersionRecord record) => new AppVersion(this, CreateDataRepository<AppVersionRecord>(), record);

        private AppRoleRepository appRoleRepo;
        internal AppRoleRepository RoleRepository() =>
            fetchRepo<AppRoleRecord, AppRoleRepository>(ref appRoleRepo, dataRepo => new AppRoleRepository(this, dataRepo));
        internal AppRole CreateAppRole(AppRoleRecord record) => new AppRole(CreateDataRepository<AppRoleRecord>(), record);

        private AppUserRoleRepository userRoleRepo;
        internal AppUserRoleRepository UserRoleRepository() =>
            fetchRepo<AppUserRoleRecord, AppUserRoleRepository>
            (
                ref userRoleRepo, dataRepo => new AppUserRoleRepository(this, dataRepo)
            );
        internal AppUserRole CreateAppUserRole(AppUserRoleRecord record) =>
            new AppUserRole(CreateDataRepository<AppUserRoleRecord>(), record);

        private TRepo fetchRepo<TRecord, TRepo>
        (
            ref TRepo repo,
            Func<DataRepository<TRecord>, TRepo> createRepo
        )
            where TRecord : class
        {
            return repo ?? (repo = createRepo(CreateDataRepository<TRecord>()));
        }

        protected abstract DataRepository<T> CreateDataRepository<T>() where T : class;

    }
}
