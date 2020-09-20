using System.Threading.Tasks;

namespace XTI_WebApp.Api
{
    public interface AppAction<TModel, TResult>
    {
        Task<TResult> Execute(TModel model);
    }
}
