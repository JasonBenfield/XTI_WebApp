using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Extensions
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
            await appFactory.UserRepository().User(id);

        public async Task<IAppUser> User()
        {
            IAppUser user;
            var httpUser = httpContextAccessor.HttpContext?.User;
            if (httpUser?.Identity.IsAuthenticated == true)
            {
                var userIDClaim = httpUser.Claims.First(c => c.Type == "UserID");
                var userID = int.Parse(userIDClaim.Value);
                user = await User(userID);
            }
            else
            {
                user = await appFactory.UserRepository().User(AppUserName.Anon);
            }
            return user;
        }

        public void RefreshUser(IAppUser user)
        {
        }
    }
}
