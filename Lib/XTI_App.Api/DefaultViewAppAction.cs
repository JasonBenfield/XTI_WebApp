using System.Threading.Tasks;

namespace XTI_App.Api
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
