using XTI_App;

namespace XTI_WebApp.Tests
{
    public sealed class FakeRoleNames : AppRoleNames
    {
        public static FakeRoleNames Instance = new FakeRoleNames();

        public FakeRoleNames() : base(FakeAppApi.AppKey)
        {
            Admin = Add("Admin");
            Viewer = Add("Viewer");
            Manager = Add("Manager");
        }

        public AppRoleName Admin { get; }
        public AppRoleName Viewer { get; }
        public AppRoleName Manager { get; }
    }

}
