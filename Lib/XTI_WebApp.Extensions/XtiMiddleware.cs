using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;

namespace XTI_WebApp.Extensions
{
    public sealed class XtiMiddleware
    {
        private readonly RequestDelegate _next;

        public XtiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync
        (
            HttpContext context,
            ISessionContext sessionLog,
            Clock clock
        )
        {
            await sessionLog.StartSession();
            await sessionLog.StartRequest();
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await handleError(context, clock, sessionLog.CurrentRequest, ex);
            }
            finally
            {
                await sessionLog.CurrentRequest.End(clock.Now());
            }
        }

        private async Task handleError(HttpContext context, Clock clock, AppRequest request, Exception ex)
        {
            await logException(clock, request, ex);
            context.Response.StatusCode = getErrorStatusCode(ex);
            context.Response.ContentType = "application/json";
            var errors = new ResultContainer<ErrorModel[]>(getErrors(ex));
            var serializedErrors = JsonSerializer.Serialize(errors);
            await context.Response.WriteAsync(serializedErrors);
        }

        private static async Task logException(Clock clock, AppRequest request, Exception ex)
        {
            AppEventSeverity severity;
            string caption;
            var now = clock.Now();
            if (ex is ValidationFailedException)
            {
                severity = AppEventSeverity.Values.ValidationFailed;
                caption = "Validation Failed";
            }
            else if (ex is AccessDeniedException accessDeniedException)
            {
                severity = AppEventSeverity.Values.AccessDenied;
                caption = accessDeniedException.DisplayMessage;
            }
            else if (ex is AppException appException)
            {
                severity = AppEventSeverity.Values.AppError;
                caption = appException.DisplayMessage;
            }
            else
            {
                severity = AppEventSeverity.Values.CriticalError;
                caption = "An unexpected error occurred";
            }
            await request.LogException(Guid.NewGuid().ToString("N"), severity, now, ex, caption);
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

        private ErrorModel[] getErrors(Exception ex)
        {
            ErrorModel[] errors;
            if (ex is AppException appException)
            {
                if (ex is ValidationFailedException validationFailedException)
                {
                    errors = validationFailedException.Errors.ToArray();
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
