using System.Threading.Tasks;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class DefaultViewAppAction<TModel> : AppAction<TModel, WebViewResult>
    {
        public Task<WebViewResult> Execute(TModel model)
        {
            var result = new WebViewResult("Index");
            return Task.FromResult(result);
        }
    }
}
