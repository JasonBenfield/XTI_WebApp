using System.Threading.Tasks;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class PartialViewAppAction<TModel> : AppAction<TModel, WebPartialViewResult>
    {
        private readonly string viewName;

        public PartialViewAppAction(string viewName)
        {
            this.viewName = viewName?.Trim() ?? "";
        }

        public Task<WebPartialViewResult> Execute(TModel model)
        {
            return Task.FromResult(new WebPartialViewResult(viewName));
        }
    }
}
