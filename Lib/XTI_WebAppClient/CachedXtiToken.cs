using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace XTI_WebAppClient
{
    public sealed class CachedXtiToken : IXtiToken
    {
        private readonly IMemoryCache cache;
        private readonly IXtiToken source;

        public CachedXtiToken(IMemoryCache cache, IXtiToken source)
        {
            this.cache = cache;
            this.source = source;
        }

        public void Reset()
        {
            source.Reset();
            cache.Set("xti_token", "");
        }

        public async Task<string> Value()
        {
            if (!cache.TryGetValue<string>("xti_token", out var token))
            {
                token = await source.Value();
                cache.Set("xti_token", token);
            }
            return token;
        }
    }
}
