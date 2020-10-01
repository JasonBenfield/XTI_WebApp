using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_App.Api
{
    public class AppApiGroup
    {
        public AppApiGroup(AppApi api, string groupName, ResourceAccess access, IAppApiUser user)
        {
            Name = api.Name.WithGroup(groupName);
            Access = access;
            this.user = user;
        }

        private readonly IAppApiUser user;
        private readonly Dictionary<string, AppApiAction> actions = new Dictionary<string, AppApiAction>();

        public XtiPath Name { get; }
        public ResourceAccess Access { get; }

        public Task<bool> HasAccess() => user.HasAccess(Access);

        public async Task EnsureUserHasAccess()
        {
            var hasAccess = await HasAccess();
            if (!hasAccess)
            {
                throw new AccessDeniedException(Name);
            }
        }

        protected AppApiAction<EmptyRequest, AppActionViewResult> AddDefaultView()
        {
            return AddDefaultView<EmptyRequest>(null);
        }

        protected AppApiAction<TModel, AppActionViewResult> AddDefaultView<TModel>
        (
            Func<IAppApiUser, AppActionValidation<TModel>> createValidation
        )
        {
            return AddView
            (
                "Index",
                createValidation,
                (u) => new DefaultViewAppAction<TModel>()
            );
        }

        protected AppApiAction<TModel, AppActionViewResult> AddView<TModel>
        (
            string actionName,
            Func<IAppApiUser, AppAction<TModel, AppActionViewResult>> createAction
        )
        {
            return AddView(actionName, null, createAction);
        }

        protected AppApiAction<TModel, AppActionViewResult> AddView<TModel>
        (
            string actionName,
            Func<IAppApiUser, AppActionValidation<TModel>> createValidation,
            Func<IAppApiUser, AppAction<TModel, AppActionViewResult>> createAction
        )
        {
            return AddView(actionName, Access, createValidation, createAction);
        }

        protected AppApiAction<TModel, AppActionViewResult> AddView<TModel>
        (
            string actionName,
            ResourceAccess access,
            Func<IAppApiUser, AppActionValidation<TModel>> createValidation,
            Func<IAppApiUser, AppAction<TModel, AppActionViewResult>> createAction
        )
        {
            return AddAction
            (
                actionName,
                access,
                createValidation,
                createAction,
                null
            );
        }

        protected AppApiAction<TModel, AppActionRedirectResult> AddRedirect<TModel>
        (
            string actionName,
            Func<IAppApiUser, AppAction<TModel, AppActionRedirectResult>> createAction
        )
        {
            return AddRedirect(actionName, null, createAction);
        }

        protected AppApiAction<TModel, AppActionRedirectResult> AddRedirect<TModel>
        (
            string actionName,
            Func<IAppApiUser, AppActionValidation<TModel>> createValidation,
            Func<IAppApiUser, AppAction<TModel, AppActionRedirectResult>> createAction
        )
        {
            return AddRedirect(actionName, Access, createValidation, createAction);
        }

        protected AppApiAction<TModel, AppActionRedirectResult> AddRedirect<TModel>
        (
            string actionName,
            ResourceAccess access,
            Func<IAppApiUser, AppActionValidation<TModel>> createValidation,
            Func<IAppApiUser, AppAction<TModel, AppActionRedirectResult>> createAction
        )
        {
            return AddAction
            (
                actionName,
                access,
                createValidation,
                createAction,
                null
            );
        }

        protected AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            Func<IAppApiUser, AppActionValidation<TModel>> createValidation,
            Func<IAppApiUser, AppAction<TModel, TResult>> createExecution,
            string friendlyName = null
        )
        {
            return AddAction(actionName, Access, createValidation, createExecution, friendlyName);
        }

        protected AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            Func<IAppApiUser, AppAction<TModel, TResult>> createExecution,
            string friendlyName = null
        )
        {
            return AddAction(actionName, Access, null, createExecution, friendlyName);
        }

        protected AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            ResourceAccess access,
            Func<IAppApiUser, AppAction<TModel, TResult>> createExecution,
            string friendlyName = null
        )
        {
            return AddAction
            (
                actionName,
                access,
                null,
                createExecution,
                friendlyName
            );
        }

        protected AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            ResourceAccess access,
            Func<IAppApiUser, AppActionValidation<TModel>> createValidation,
            Func<IAppApiUser, AppAction<TModel, TResult>> createExecution,
            string friendlyName = null
        )
        {
            if (string.IsNullOrWhiteSpace(actionName)) { throw new ArgumentException($"{nameof(actionName)} is required"); }
            var action = new AppApiAction<TModel, TResult>
            (
                Name.WithAction(actionName),
                access,
                user,
                createValidation ?? (u => new AppActionValidationNotRequired<TModel>()),
                createExecution ?? (u => new EmptyAppAction<TModel, TResult>()),
                friendlyName ?? new FriendlyNameFromActionName(actionName).Value
            );
            actions.Add(action.Name.Action.ToLower(), action);
            return action;
        }

        public IEnumerable<AppApiAction> Actions() => actions.Values.ToArray();

        public AppApiAction<TModel, TResult> Action<TModel, TResult>(string actionName)
        {
            return (AppApiAction<TModel, TResult>)actions[actionName.ToLower()];
        }

        public AppApiGroupTemplate Template()
        {
            var actionTemplates = Actions().Select(a => a.Template());
            return new AppApiGroupTemplate(Name.Group, Access, actionTemplates);
        }
    }
}
