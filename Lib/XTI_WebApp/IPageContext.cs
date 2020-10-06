namespace XTI_WebApp
{
    public interface IPageContext
    {
        string BaseUrl { get; }
        string CacheBust { get; }
        string EnvironmentName { get; }
        string AppTitle { get; }
        string PageTitle { get; set; }
    }
}
