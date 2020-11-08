using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.TestFakes
{
    public sealed class FakeAppApiTemplateFactory : IAppApiTemplateFactory
    {
        public AppApiTemplate Create()
        {
            var api = new FakeAppApi(FakeAppKey.AppKey, AppVersionKey.Current, new AppApiSuperUser());
            return api.Template();
        }
    }
}
