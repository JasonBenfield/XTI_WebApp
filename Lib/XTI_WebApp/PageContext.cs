using System.Text.Json;

namespace XTI_WebApp
{
    public sealed class PageContext
    {
        public string BaseUrl { get; set; }
        public string CacheBust { get; set; }

        public string Serialize() => JsonSerializer.Serialize(this);
    }
}
