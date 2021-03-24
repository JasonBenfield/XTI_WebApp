using XTI_App.Abstractions;

namespace XTI_WebApp.TestFakes
{
    public static class FakeInfo
    {
        public static readonly AppKey AppKey = new AppKey(new AppName("Fake"), AppType.Values.WebApp);
        public static readonly FakeAppRoles Roles = FakeAppRoles.Instance;
    }
}
