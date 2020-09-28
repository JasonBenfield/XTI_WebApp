namespace XTI_WebApp
{
    public sealed class WebAppOptions
    {
        public static readonly string WebApp = "WebApp";

        public string BaseUrl { get; set; }
        public string KeyFolder { get; set; }
        public string CacheBust { get; set; }
    }
}
