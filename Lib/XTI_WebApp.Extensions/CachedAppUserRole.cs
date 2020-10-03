using XTI_App;

namespace XTI_WebApp.Extensions
{
    public sealed class CachedAppUserRole : IAppUserRole
    {
        public CachedAppUserRole(IAppUserRole source)
        {
            RoleID = source.RoleID;
            modifier = source.Modifier();
        }

        public int RoleID { get; }

        public bool IsRole(IAppRole appRole) => appRole.ID == RoleID;
        private readonly AccessModifier modifier;
        public AccessModifier Modifier() => modifier;
    }
}
