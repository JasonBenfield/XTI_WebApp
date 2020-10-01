namespace XTI_App
{
    public sealed class AppUserRole
    {
        private readonly AppUserRoleRecord record;

        internal AppUserRole(AppUserRoleRecord record)
        {
            this.record = record ?? new AppUserRoleRecord();
        }

        public bool IsRole(AppRole appRole) => appRole.ID == record.RoleID;

        public override string ToString() => $"{nameof(AppUserRole)} {record.ID}";
    }
}
