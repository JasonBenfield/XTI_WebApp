using System.Threading.Tasks;

namespace XTI_WebApp.Api
{
    public sealed class EmptyAppAction<TModel, TResult> : AppAction<TModel, TResult>
    {
        public Task<TResult> Execute(TModel model) => Task.FromResult(default(TResult));
    }
}
