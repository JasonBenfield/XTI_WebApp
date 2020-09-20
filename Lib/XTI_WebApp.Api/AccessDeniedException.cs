using XTI_App;

namespace XTI_WebApp.Api
{
    public sealed class AccessDeniedException : AppException
    {
        public AccessDeniedException(AppResourceName resourceName)
            : base($"Access denied to {resourceName}", "Access Denied")
        {
            ResourceName = resourceName;
        }

        public AppResourceName ResourceName { get; }
    }
}
