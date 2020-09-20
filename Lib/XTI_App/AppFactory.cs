using System;
using System.Threading.Tasks;

namespace XTI_App
{
    public abstract class AppFactory
    {
        protected AppFactory() { }

        private AppUserRepository userRepo;
        public AppUserRepository UserRepository() =>
            userRepo ?? (userRepo = CreateAppUserRepository());
        private AppUserRepository CreateAppUserRepository() =>
            new AppUserRepository(this, CreateDataRepository<AppUserRecord>());
        internal AppUser CreateAppUser(AppUserRecord record) => new AppUser(record);

        private AppSessionRepository sessionRepo;
        public AppSessionRepository SessionRepository() =>
            sessionRepo ?? (sessionRepo = CreateAppSessionRepository());
        private AppSessionRepository CreateAppSessionRepository() =>
            new AppSessionRepository(this, CreateDataRepository<AppSessionRecord>());
        internal AppSession CreateAppSession(AppSessionRecord record) =>
            new AppSession(this, CreateDataRepository<AppSessionRecord>(), record);

        private AppRequestRepository requestRepo;
        public AppRequestRepository RequestRepository() =>
            requestRepo ?? (requestRepo = CreateAppRequestRepository());
        private AppRequestRepository CreateAppRequestRepository() =>
            new AppRequestRepository(this, CreateDataRepository<AppRequestRecord>());
        internal AppRequest CreateAppRequest(AppRequestRecord record) => new AppRequest(this, record);

        private AppEventRepository eventRepo;
        public AppEventRepository EventRepository() =>
            eventRepo ?? (eventRepo = CreateEventRepository());
        private AppEventRepository CreateEventRepository() =>
            new AppEventRepository(this, CreateDataRepository<AppEventRecord>());
        internal AppEvent CreateEvent(AppEventRecord record) => new AppEvent(record);

        protected abstract DataRepository<T> CreateDataRepository<T>() where T : class;

    }
}
