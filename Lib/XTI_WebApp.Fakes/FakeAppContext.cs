using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Fakes
{
    public sealed class FakeAppContext : IAppContext
    {
        private IApp app;

        public Task<IApp> App() => Task.FromResult(app);

        public void SetApp(IApp app) => this.app = app;
    }
}
