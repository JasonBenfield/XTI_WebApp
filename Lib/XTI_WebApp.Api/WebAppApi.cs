using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public class WebAppApi : AppApi
    {
        protected WebAppApi(AppKey appKey, AppVersionKey versionKey, IAppApiUser user, ResourceAccess access)
            : base(appKey, versionKey, user, access)
        {
            User = AddGroup(u => new UserGroup(this, u));
        }

        public UserGroup User { get; }
    }
}
