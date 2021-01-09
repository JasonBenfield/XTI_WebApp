using System.Threading.Tasks;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class TitledViewAppAction<TModel> : AppAction<TModel, WebViewResult>
    {
        private readonly IPageContext pageContext;
        private readonly string viewName;
        private readonly string pageTitle;

        public TitledViewAppAction(IPageContext pageContext, string viewName, string pageTitle)
        {
            this.pageContext = pageContext;
            this.viewName = viewName?.Trim() ?? "";
            this.pageTitle = pageTitle?.Trim() ?? "";
        }

        public Task<WebViewResult> Execute(TModel model)
        {
            pageContext.PageTitle = pageTitle;
            return Task.FromResult(new WebViewResult(viewName));
        }
    }
}
