using XTI_App.Api;

namespace FakeWebApp.Api
{
    public sealed class FakeAppApiTemplateFactory
    {
        public AppApiTemplate Create()
        {
            var api = new FakeAppApi(new AppApiSuperUser());
            return api.Template();
        }
    }
}
