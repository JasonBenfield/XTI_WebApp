using System.Threading.Tasks;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class DefaultViewAppAction<TModel> : AppAction<TModel, AppActionViewResult>
    {
        public Task<AppActionViewResult> Execute(TModel model)
        {
            var result = new AppActionViewResult("Index");
            return Task.FromResult(result);
        }
    }
}
