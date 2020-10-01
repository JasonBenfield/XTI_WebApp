using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Fakes
{
    public sealed class FakeSessionContext : ISessionContext
    {
        private IAppUser user;

        public Task<IAppUser> User() => Task.FromResult(user);

        public void SetUser(IAppUser user) => this.user = user;

        private IAppSession session;
        public Task<IAppSession> Session() => Task.FromResult(session);

        public void SetSession(IAppSession session) => this.session = session;
    }
}
