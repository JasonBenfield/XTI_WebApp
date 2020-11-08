using XTI_App;

namespace XTI_WebApp.Api
{
    public sealed class CachedAppVersion : IAppVersion
    {
        public CachedAppVersion(IAppVersion appVersion)
        {
            ID = appVersion.ID;
        }

        public EntityID ID { get; }
    }
}
