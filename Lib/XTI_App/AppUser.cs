﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppUser : IAppUser
    {
        private readonly DataRepository<AppUserRecord> repo;
        private readonly AppFactory factory;
        private readonly AppUserRecord record;

        internal AppUser(DataRepository<AppUserRecord> repo, AppFactory factory, AppUserRecord record)
        {
            this.repo = repo;
            this.factory = factory;
            this.record = record ?? new AppUserRecord();
        }

        public int ID { get => record.ID; }
        public AppUserName UserName() => new AppUserName(record.UserName);
        public bool Exists() => ID > 0;

        public bool IsPasswordCorrect(IHashedPassword hashedPassword) =>
            hashedPassword.Equals(record.Password);

        public override string ToString() => $"{nameof(AppUser)} {ID}";

        public Task<AppUserRole> AddRole(AppRole role) =>
            AddRole(role, AccessModifier.Default);
        public Task<AppUserRole> AddRole(AppRole role, AccessModifier modifier) =>
            factory.UserRoles().Add(this, role, modifier);

        public Task<IEnumerable<AppUserRole>> RolesForApp(App app) =>
            factory.UserRoles().RolesForUser(this, app);

        async Task<IEnumerable<IAppUserRole>> IAppUser.RolesForApp(IApp app) =>
            await factory.UserRoles().RolesForUser(this, app);

        public Task RemoveRole(AppUserRole userAdminRole) => userAdminRole.Delete();

        public Task ChangePassword(IHashedPassword password)
        {
            return repo.Update(record, u => u.Password = password.Value());
        }
    }
}
