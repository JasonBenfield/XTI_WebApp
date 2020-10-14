namespace XTI_WebApp.Api
{
    public sealed class AppActionViewResult
    {
        public AppActionViewResult(string viewName)
        {
            ViewName = viewName;
        }

        public string ViewName { get; }
    }
}
