using System;
using System.Threading.Tasks;
using XTI_App;

namespace XTI_App.Api
{
    public interface AppApiAction
    {
        XtiPath Name { get; }
        string FriendlyName { get; }
        ResourceAccess Access { get; }

        Task<bool> HasAccess();

        Task<object> Execute(object model);

        AppApiActionTemplate Template();
    }

    public sealed class AppApiAction<TModel, TResult> : AppApiAction
    {
        public AppApiAction
        (
            XtiPath name,
            ResourceAccess access,
            AppApiUser user,
            Func<AppApiUser, AppActionValidation<TModel>> createValidation,
            Func<AppApiUser, AppAction<TModel, TResult>> createExecution,
            string friendlyName
        )
        {
            name.EnsureActionResource();
            Access = access;
            Name = name;
            FriendlyName = friendlyName;
            this.user = user;
            this.createValidation = createValidation;
            this.createExecution = createExecution;
        }

        private readonly AppApiUser user;
        private readonly Func<AppApiUser, AppActionValidation<TModel>> createValidation;
        private readonly Func<AppApiUser, AppAction<TModel, TResult>> createExecution;

        public XtiPath Name { get; }
        public string FriendlyName { get; }
        public ResourceAccess Access { get; }

        public Task<bool> HasAccess() => user.HasAccess(Access);

        public async Task<object> Execute(object model) => await Execute((TModel)model);

        public async Task<ResultContainer<TResult>> Execute(TModel model)
        {
            await EnsureUserHasAccess();
            var errors = new ErrorList();
            var validation = createValidation(user);
            await validation.Validate(errors, model);
            if (errors.Any())
            {
                throw new ValidationFailedException(errors.Errors());
            }
            var action = createExecution(user);
            var actionResult = await action.Execute(model);
            return new ResultContainer<TResult>(actionResult);
        }

        private async Task EnsureUserHasAccess()
        {
            var hasAccess = await HasAccess();
            if (!hasAccess)
            {
                throw new AccessDeniedException(Name);
            }
        }

        public AppApiActionTemplate Template()
        {
            var modelTemplate = new ValueTemplateFromType(typeof(TModel)).Template();
            var resultTemplate = new ValueTemplateFromType(typeof(TResult)).Template();
            return new AppApiActionTemplate(Name.Action, FriendlyName, Access, modelTemplate, resultTemplate);
        }
    }
}
