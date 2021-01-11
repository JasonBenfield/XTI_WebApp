using System;
using System.Collections.Generic;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class WebAppApiActionCollection : IAppApiActionCollection
    {
        private readonly AppApiActionCollection actions;

        public WebAppApiActionCollection(XtiPath appPath, ResourceAccess access, IAppApiUser user)
        {
            actions = new AppApiActionCollection(appPath, access, user);
        }

        public IEnumerable<IAppApiAction> Actions() => actions.Actions();

        public AppApiAction<TModel, TResult> Action<TModel, TResult>(string actionName)
            => actions.Action<TModel, TResult>(actionName);

        public AppApiAction<TModel, TResult> Add<TModel, TResult>(AppApiAction<TModel, TResult> action)
            => actions.Add(action);

        public AppApiAction<EmptyRequest, WebViewResult> AddDefaultView() =>
            AddDefaultView(AppApiActionCollection.defaultCreateValidation<EmptyRequest>());

        public AppApiAction<TModel, WebViewResult> AddDefaultView<TModel>() =>
            AddDefaultView(AppApiActionCollection.defaultCreateValidation<TModel>());

        public AppApiAction<TModel, WebViewResult> AddDefaultView<TModel>
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

        public AppApiAction<TModel, WebViewResult> AddView<TModel>
        (
            string actionName,
            Func<AppAction<TModel, WebViewResult>> createAction
        )
        {
            return AddView(actionName, AppApiActionCollection.defaultCreateValidation<TModel>(), createAction);
        }

        public AppApiAction<TModel, WebViewResult> AddView<TModel>
        (
            string actionName,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, WebViewResult>> createAction
        )
        {
            return AddAction
            (
                actionName,
                createValidation,
                createAction,
                ""
            );
        }

        public AppApiAction<TModel, WebViewResult> AddView<TModel>
        (
            string actionName,
            ResourceAccess access,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, WebViewResult>> createAction
        )
        {
            return AddAction
            (
                actionName,
                access,
                createValidation,
                createAction,
                ""
            );
        }

        public AppApiAction<TModel, WebPartialViewResult> AddPartialView<TModel>
        (
            string actionName,
            Func<AppAction<TModel, WebPartialViewResult>> createAction
        )
        {
            return AddPartialView(actionName, AppApiActionCollection.defaultCreateValidation<TModel>(), createAction);
        }

        public AppApiAction<TModel, WebPartialViewResult> AddPartialView<TModel>
        (
            string actionName,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, WebPartialViewResult>> createAction
        )
        {
            return AddAction
            (
                actionName,
                createValidation,
                createAction,
                ""
            );
        }

        public AppApiAction<TModel, WebPartialViewResult> AddPartialView<TModel>
        (
            string actionName,
            ResourceAccess access,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, WebPartialViewResult>> createAction
        )
        {
            return AddAction
            (
                actionName,
                access,
                createValidation,
                createAction,
                ""
            );
        }

        public AppApiAction<TModel, WebRedirectResult> AddRedirect<TModel>
        (
            string actionName,
            Func<AppAction<TModel, WebRedirectResult>> createAction
        )
        {
            return AddRedirect(actionName, AppApiActionCollection.defaultCreateValidation<TModel>(), createAction);
        }

        public AppApiAction<TModel, WebRedirectResult> AddRedirect<TModel>
        (
            string actionName,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, WebRedirectResult>> createAction
        )
        {
            return AddAction
            (
                actionName,
                createValidation,
                createAction,
                ""
            );
        }

        public AppApiAction<TModel, WebRedirectResult> AddRedirect<TModel>
        (
            string actionName,
            ResourceAccess access,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, WebRedirectResult>> createAction
        )
        {
            return AddAction
            (
                actionName,
                access,
                createValidation,
                createAction,
                ""
            );
        }

        public AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, TResult>> createExecution,
            string friendlyName = ""
        ) => actions.Add(actionName, createValidation, createExecution, friendlyName);

        public AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            Func<AppAction<TModel, TResult>> createExecution,
            string friendlyName = ""
        ) => actions.Add(actionName, createExecution, friendlyName);

        public AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            ResourceAccess access,
            Func<AppAction<TModel, TResult>> createExecution,
            string friendlyName = ""
        ) => actions.Add(actionName, access, createExecution, friendlyName);

        public AppApiAction<TModel, TResult> AddAction<TModel, TResult>
        (
            string actionName,
            ResourceAccess access,
            Func<AppActionValidation<TModel>> createValidation,
            Func<AppAction<TModel, TResult>> createExecution,
            string friendlyName = null
        ) => actions.Add(actionName, access, createValidation, createExecution, friendlyName);

    }
}
