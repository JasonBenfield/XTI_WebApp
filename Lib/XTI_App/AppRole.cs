namespace XTI_App
{
    public sealed class AppRole
    {
        private readonly AppRoleRecord record;

        internal AppRole(AppRoleRecord record)
        {
            this.record = record;
        }

        public AppRoleName Name() => new AppRoleName(record.Name);

        public override string ToString() => $"{nameof(AppRole)} {record.ID}";
    }
}
