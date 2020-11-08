using XTI_App;

namespace XTI_WebApp.Api
{
    public sealed class CachedAppUserRole : IAppUserRole
    {
        public CachedAppUserRole(IAppUserRole source)
        {
            RoleID = source.RoleID;
        }

        public int RoleID { get; }

        public bool IsRole(IAppRole appRole) => appRole.ID.Value == RoleID;
    }
}
