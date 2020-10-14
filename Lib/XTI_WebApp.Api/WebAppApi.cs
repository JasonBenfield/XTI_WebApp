using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public class WebAppApi : AppApi
    {
        protected WebAppApi(string appKey, string version, IAppApiUser user, ResourceAccess access)
            : base(appKey, version, user, access)
        {
            User = AddGroup(u => new UserGroup(this, u));
        }

        public UserGroup User { get; }
    }
}
