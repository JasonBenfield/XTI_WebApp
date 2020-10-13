using XTI_App;

namespace XTI_WebApp.TestFakes
{
    public sealed class FakeAppRoles : AppRoleNames
    {
        public static readonly FakeAppRoles Instance = new FakeAppRoles();

        public FakeAppRoles() : base(new AppKey("Fake"))
        {
            Admin = Add("Admin");
            Viewer = Add("Viewer");
        }
        public AppRoleName Admin { get; }
        public AppRoleName Viewer { get; }
    }
}
