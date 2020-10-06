using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_App.Api
{
    public class AppApiGroup
    {
        public AppApiGroup
        (
            AppApi api,
            string groupName,
            bool hasModifier,
            ResourceAccess access,
            IAppApiUser user
        )
        {
            Name = api.Name.WithGroup(groupName);
            this.hasModifier = hasModifier;
            Access = access ?? ResourceAccess.AllowAuthenticated();
            this.user = user;
        }

        private readonly bool hasModifier;
        private readonly IAppApiUser user;
        private readonly Dictionary<string, IAppApiAction> actions = new Dictionary<string, IAppApiAction>();

        public XtiPath Name { get; }
        public ResourceAccess Access { get; }

        public Task<bool> HasAccess() => HasAccess(AccessModifier.Default);
        public Task<bool> HasAccess(AccessModifier modifier) => user.HasAccess(Access, modifier);

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
            Func<AppActionValidation<TModel>> createValidation
        )
        {
            return AddView
            (
                "Index",
                createValidation,
                () => new DefaultViewAppAction<TModel>()
            );
        }

        protected AppApiAction<TModel, AppActionViewResult> AddView<TModel>
        (
            string actionName,
            Func<AppAction<TModel, AppActionViewResult>> createAction
        )
        {
            return AddView(actionName, null, createAction);
        }

        protected AppApiAction<TModel, AppActionViewResult> AddView<TModel>
        (
            string actionName,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, AppActionViewResult>> createAction
        )
        {
            return AddView(actionName, Access, createValidation, createAction);
        }

        protected AppApiAction<TModel, AppActionViewResult> AddView<TModel>
        (
            string actionName,
            ResourceAccess access,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, AppActionViewResult>> createAction
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
            Func<AppAction<TModel, AppActionRedirectResult>> createAction
        )
        {
            return AddRedirect(actionName, null, createAction);
        }

        protected AppApiAction<TModel, AppActionRedirectResult> AddRedirect<TModel>
        (
            string actionName,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, AppActionRedirectResult>> createAction
        )
        {
            return AddRedirect(actionName, Access, createValidation, createAction);
        }

        protected AppApiAction<TModel, AppActionRedirectResult> AddRedirect<TModel>
        (
            string actionName,
            ResourceAccess access,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, AppActionRedirectResult>> createAction
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
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, TResult>> createExecution,
            string friendlyName = null
        )
        {
            return AddAction(actionName, Access, createValidation, createExecution, friendlyName);
        }

        protected AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            Func<AppAction<TModel, TResult>> createExecution,
            string friendlyName = null
        )
        {
            return AddAction(actionName, Access, null, createExecution, friendlyName);
        }

        protected AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            ResourceAccess access,
            Func<AppAction<TModel, TResult>> createExecution,
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
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, TResult>> createExecution,
            string friendlyName = null
        )
        {
            if (string.IsNullOrWhiteSpace(actionName)) { throw new ArgumentException($"{nameof(actionName)} is required"); }
            var action = new AppApiAction<TModel, TResult>
            (
                Name.WithAction(actionName),
                access,
                user,
                createValidation ?? (() => new AppActionValidationNotRequired<TModel>()),
                createExecution ?? (() => new EmptyAppAction<TModel, TResult>()),
                friendlyName ?? new FriendlyNameFromActionName(actionName).Value
            );
            actions.Add(action.Name.Action.ToLower(), action);
            return action;
        }

        public IEnumerable<IAppApiAction> Actions() => actions.Values.ToArray();

        public AppApiAction<TModel, TResult> Action<TModel, TResult>(string actionName)
        {
            return (AppApiAction<TModel, TResult>)actions[actionName.ToLower()];
        }

        public AppApiGroupTemplate Template()
        {
            var actionTemplates = Actions().Select(a => a.Template());
            return new AppApiGroupTemplate(Name.Group, hasModifier, Access, actionTemplates);
        }
    }
}
