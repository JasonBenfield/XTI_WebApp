using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class WebUserContext : IUserContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AppFactory appFactory;

        public WebUserContext(IHttpContextAccessor httpContextAccessor, AppFactory appFactory)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.appFactory = appFactory;
        }

        public async Task<IAppUser> User(int id) =>
            await appFactory.Users().User(id);

        public async Task<IAppUser> User() => await retrieveUser();

        public Task<AppUser> UncachedUser() => retrieveUser();

        private async Task<AppUser> retrieveUser()
        {
            AppUser user;
            var httpUser = httpContextAccessor.HttpContext?.User;
            if (httpUser?.Identity.IsAuthenticated == true)
            {
                var xtiClaims = new XtiClaims(httpContextAccessor);
                user = await appFactory.Users().User(xtiClaims.UserID());
            }
            else
            {
                user = await appFactory.Users().User(AppUserName.Anon);
            }
            return user;
        }

        public void RefreshUser(IAppUser user)
        {
        }

    }
}
