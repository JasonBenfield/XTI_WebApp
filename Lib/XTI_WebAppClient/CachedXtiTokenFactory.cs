using Microsoft.Extensions.Caching.Memory;

namespace XTI_WebAppClient
{
    public sealed class CachedXtiTokenFactory : IXtiTokenFactory
    {
        private readonly IMemoryCache cache;
        private readonly IXtiTokenFactory sourceFactory;

        public CachedXtiTokenFactory(IMemoryCache cache, IXtiTokenFactory sourceFactory)
        {
            this.cache = cache;
            this.sourceFactory = sourceFactory;
        }

        public IXtiToken Create(IAuthClient authClient)
            => new CachedXtiToken(cache, sourceFactory.Create(authClient));
    }
}
