using XTI_App;
using XTI_App.Abstractions;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public class WebAppApiWrapper : AppApiWrapper
    {
        protected WebAppApiWrapper(AppApi source)
            : base(source)
        {
            User = new UserGroup
            (
                source.AddGroup
                (
                    nameof(User),
                    ModifierCategoryName.Default,
                    ResourceAccess.AllowAuthenticated()
                )
            );
        }

        public UserGroup User { get; }
    }
}
