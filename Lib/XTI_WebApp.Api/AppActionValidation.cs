using System.Threading.Tasks;

namespace XTI_WebApp.Api
{
    public interface AppActionValidation<TModel>
    {
        Task Validate(ErrorList errors, TModel model);
    }
}
