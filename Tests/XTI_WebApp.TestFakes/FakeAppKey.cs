using XTI_App;

namespace XTI_WebApp.TestFakes
{
    public static class FakeAppKey
    {
        public static readonly AppKey AppKey = new AppKey(new AppName("Fake"), AppType.Values.WebApp);
    }
}
