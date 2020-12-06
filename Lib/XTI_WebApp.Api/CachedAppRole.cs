using XTI_App;

namespace XTI_WebApp.Api
{
    public sealed class CachedAppRole : IAppRole
    {
        private readonly AppRoleName name;

        public CachedAppRole(IAppRole source)
        {
            ID = source.ID;
            name = source.Name();
        }

        public EntityID ID { get; }

        public AppRoleName Name() => name;
    }
}
