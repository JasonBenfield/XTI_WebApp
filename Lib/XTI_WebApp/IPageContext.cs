namespace XTI_WebApp
{
    public interface IPageContext
    {
        string BaseUrl { get; }
        string CacheBust { get; }
        string EnvironmentName { get; }
        string AppTitle { get; }
        bool IsAuthenticated { get; }
        string UserName { get; }
        string PageTitle { get; set; }
    }
}
