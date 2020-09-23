using XTI_WebApp.Api;

namespace FakeWebApp.Api
{
    public sealed class FakeAppApiTemplateFactory
    {
        public AppApiTemplate Create()
        {
            var api = new FakeAppApi(new SuperUser());
            return api.Template();
        }
    }
}
