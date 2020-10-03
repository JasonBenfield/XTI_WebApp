using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppUser : IAppUser
    {
        internal AppUser(AppFactory factory, AppUserRecord record)
        {
            this.factory = factory;
            this.record = record ?? new AppUserRecord();
        }

        private readonly AppFactory factory;
        private readonly AppUserRecord record;

        public int ID { get => record.ID; }
        public AppUserName UserName() => new AppUserName(record.UserName);
        public bool IsUnknown() => UserName().Equals(AppUserName.Unknown);

        public bool IsPasswordCorrect(IHashedPassword hashedPassword) =>
            hashedPassword.Equals(record.Password);

        public override string ToString() => $"{nameof(AppUser)} {ID}";

        public Task<AppUserRole> AddRole(AppRole role) =>
            AddRole(role, AccessModifier.Default);
        public Task<AppUserRole> AddRole(AppRole role, AccessModifier modifier) =>
            factory.UserRoleRepository().Add(this, role, modifier);

        public Task<IEnumerable<AppUserRole>> RolesForApp(App app) =>
            factory.UserRoleRepository().RolesForUser(this, app);

        async Task<IEnumerable<IAppUserRole>> IAppUser.RolesForApp(IApp app) =>
            await factory.UserRoleRepository().RolesForUser(this, app);

        public Task RemoveRole(AppUserRole userAdminRole) => userAdminRole.Delete();
    }
}
