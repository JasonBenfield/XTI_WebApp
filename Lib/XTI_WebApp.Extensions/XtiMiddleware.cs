using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Extensions
{
    public sealed class XtiMiddleware
    {
        public XtiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private readonly RequestDelegate _next;

        public async Task InvokeAsync
        (
            HttpContext context,
            Clock clock,
            AppFactory appFactory,
            AnonClient anonClient,
            IAppContext appContext,
            ISessionContext sessionContext,
            XtiPath xtiPath
        )
        {
            var session = await sessionContext.Session();
            var version = await retrieveVersion(appContext, xtiPath);
            var request = await session.LogRequest(version, context.Request.Path, clock.Now());
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await handleError(context, clock, request, ex);
            }
            finally
            {
                await request.End(clock.Now());
            }
        }

        private static async Task<IAppVersion> retrieveVersion(IAppContext appContext, XtiPath xtiPath)
        {
            var app = await appContext.App();
            IAppVersion version;
            if (xtiPath.IsCurrentVersion())
            {
                version = await app.CurrentVersion();
            }
            else
            {
                version = await app.Version(xtiPath.VersionID());
            }
            return version;
        }

        private async Task handleError(HttpContext context, Clock clock, AppRequest request, Exception ex)
        {
            await request.LogCriticalException(clock.Now(), ex, "An unexpected error occurred");
            context.Response.StatusCode = getErrorStatusCode(ex);
            context.Response.ContentType = "application/json";
            var errors = getErrors(ex);
            var serializedErrors = JsonSerializer.Serialize(errors);
            await context.Response.WriteAsync(serializedErrors);
        }

        private int getErrorStatusCode(Exception ex)
        {
            int statusCode;
            if (ex is AppException)
            {
                if (ex is AccessDeniedException)
                {
                    statusCode = StatusCodes.Status403Forbidden;
                }
                else
                {
                    statusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
            {
                statusCode = StatusCodes.Status500InternalServerError;
            }
            return statusCode;
        }

        private IEnumerable<ErrorModel> getErrors(Exception ex)
        {
            IEnumerable<ErrorModel> errors;
            if (ex is AppException appException)
            {
                if (ex is ValidationFailedException validationFailedException)
                {
                    errors = validationFailedException.Errors;
                }
                else
                {
                    errors = new[] { new ErrorModel(appException.DisplayMessage) };
                }
            }
            else
            {
                errors = new[] { new ErrorModel("An unexpected error occurred") };
            }
            return errors;
        }

    }
}
