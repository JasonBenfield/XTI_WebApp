using XTI_App;

namespace XTI_WebApp.TestFakes
{
    public sealed class FakeAppRoles
    {
        public static readonly FakeAppRoles Instance = new FakeAppRoles();

        public AppRoleName Admin { get; } = new AppRoleName(nameof(Admin));
        public AppRoleName Viewer { get; } = new AppRoleName(nameof(Viewer));
    }
}
