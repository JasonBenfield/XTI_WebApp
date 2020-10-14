namespace XTI_WebApp.Api
{
    public sealed class AppActionRedirectResult
    {
        public AppActionRedirectResult(string url)
        {
            Url = url;
        }

        public string Url { get; }
    }
}
