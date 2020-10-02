using XTI_App;

namespace XTI_WebApp.Extensions
{
    public sealed class CachedAppVersion : IAppVersion
    {
        public CachedAppVersion(IAppVersion appVersion)
        {
            ID = appVersion.ID;
        }

        public int ID { get; }
    }
}
