using System.Threading.Tasks;

namespace XTI_WebApp.Api
{
    public sealed class AppActionValidationNotRequired<TModel> : AppActionValidation<TModel>
    {
        public Task Validate(ErrorList errors, TModel model)
        {
            return Task.CompletedTask;
        }
    }
}
