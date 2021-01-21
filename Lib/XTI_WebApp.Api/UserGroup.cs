using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class UserStartRequest
    {
        public string ReturnUrl { get; set; }
    }
    public sealed class UserGroup : AppApiGroupWrapper
    {
        public UserGroup(AppApiGroup source) : base(source)
        {
            var actions = new WebAppApiActionFactory(source);
            Index = source.AddAction(actions.DefaultView<UserStartRequest>());
        }

        public AppApiAction<UserStartRequest, WebViewResult> Index { get; }
    }
}
