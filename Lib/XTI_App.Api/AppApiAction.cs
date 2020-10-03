﻿using System;
using System.Threading.Tasks;

namespace XTI_App.Api
{
    public interface AppApiAction
    {
        XtiPath Name { get; }
        string FriendlyName { get; }
        ResourceAccess Access { get; }

        Task<bool> HasAccess(AccessModifier modifier);

        Task<object> Execute(AccessModifier modifier, object model);

        AppApiActionTemplate Template();
    }

    public sealed class AppApiAction<TModel, TResult> : AppApiAction
    {
        public AppApiAction
        (
            XtiPath name,
            ResourceAccess access,
            IAppApiUser user,
            Func<IAppApiUser, AppActionValidation<TModel>> createValidation,
            Func<IAppApiUser, AppAction<TModel, TResult>> createExecution,
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

        private readonly IAppApiUser user;
        private readonly Func<IAppApiUser, AppActionValidation<TModel>> createValidation;
        private readonly Func<IAppApiUser, AppAction<TModel, TResult>> createExecution;

        public XtiPath Name { get; }
        public string FriendlyName { get; }
        public ResourceAccess Access { get; }

        public Task<bool> HasAccess(AccessModifier modifier) =>
            user.HasAccess(Access, modifier);

        public async Task<object> Execute(AccessModifier modifier, object model) =>
            await Execute(modifier, (TModel)model);

        public Task<ResultContainer<TResult>> Execute(TModel model) =>
            Execute(AccessModifier.Default, model);

        public async Task<ResultContainer<TResult>> Execute
        (
            AccessModifier modifier, TModel model
        )
        {
            await EnsureUserHasAccess(modifier);
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

        private async Task EnsureUserHasAccess(AccessModifier modifier)
        {
            var hasAccess = await HasAccess(modifier);
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
