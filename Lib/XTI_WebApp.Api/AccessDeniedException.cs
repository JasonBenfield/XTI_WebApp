using XTI_App;

namespace XTI_WebApp.Api
{
    public sealed class AccessDeniedException : AppException
    {
        public AccessDeniedException(XtiPath resourceName)
            : base($"Access denied to {resourceName}", "Access Denied")
        {
            ResourceName = resourceName;
        }

        public XtiPath ResourceName { get; }
    }
}
