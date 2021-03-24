using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_App.EfApi;

namespace XTI_WebApp.Extensions
{
    public sealed class WebUserContext : IUserContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly DefaultUserContext userContext;

        public WebUserContext(IHttpContextAccessor httpContextAccessor, AppFactory appFactory)
        {
            this.httpContextAccessor = httpContextAccessor;
            userContext = new DefaultUserContext(appFactory, this.getUserID);
        }

        private int getUserID()
        {
            var xtiClaims = new XtiClaims(httpContextAccessor);
            return xtiClaims.UserID();
        }

        public Task<IAppUser> User(int id) => userContext.User(id);

        public Task<IAppUser> User() => userContext.User();
    }
}
