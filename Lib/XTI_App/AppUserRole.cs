namespace XTI_App
{
    public sealed class AppUserRole : IAppUserRole
    {
        private readonly AppUserRoleRecord record;

        internal AppUserRole(AppUserRoleRecord record)
        {
            this.record = record ?? new AppUserRoleRecord();
        }

        public bool IsRole(IAppRole appRole) => appRole.ID == record.RoleID;

        public override string ToString() => $"{nameof(AppUserRole)} {record.ID}";
    }
}
