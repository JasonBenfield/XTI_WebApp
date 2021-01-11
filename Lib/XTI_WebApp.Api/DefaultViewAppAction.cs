using System.Threading.Tasks;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class DefaultViewAppAction<TModel> : AppAction<TModel, WebViewResult>
    {
        private readonly ViewAppAction<TModel> action = new ViewAppAction<TModel>("Index");

        public Task<WebViewResult> Execute(TModel model) => action.Execute(model);
    }
}
