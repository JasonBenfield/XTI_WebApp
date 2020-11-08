using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class UserStartRequest
    {
        public string ReturnUrl { get; set; }
    }
    public sealed class UserGroup : AppApiGroup
    {
        public UserGroup(AppApi api, IAppApiUser user)
            : base
            (
                api,
                new NameFromGroupClassName(nameof(UserGroup)).Value,
                ModifierCategoryName.Default,
                ResourceAccess.AllowAuthenticated(),
                user,
                (name, ra, u) => new WebAppApiActionCollection(name, ra, u)
            )
        {
            var actions = Actions<WebAppApiActionCollection>();
            Index = actions.AddDefaultView<UserStartRequest>();
        }

        public AppApiAction<UserStartRequest, AppActionViewResult> Index { get; }
    }
}
