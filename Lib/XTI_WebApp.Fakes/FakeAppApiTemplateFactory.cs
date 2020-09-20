using XTI_WebApp.Api;

namespace XTI_WebApp.Fakes
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
