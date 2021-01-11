using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace XTI_WebApp.Extensions
{
    public sealed class XtiMiddleware
    {
        private readonly RequestDelegate _next;

        public XtiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, CurrentSession currentSession, TempLogSession sessionLog, IAnonClient anonClient, Clock clock)
        {
            anonClient.Load();
            if (isAnonSessionExpired(anonClient, clock))
            {
                expireAnonSession(anonClient);
            }
            if (context.User.Identity.IsAuthenticated)
            {
                currentSession.SessionKey = new XtiClaims(context).SessionKey();
            }
            else
            {
                currentSession.SessionKey = anonClient.SessionKey;
            }
            var session = await sessionLog.StartSession();
            if (anonClient.SessionKey != session.SessionKey)
            {
                anonClient.Persist(session.SessionKey, clock.Now().AddHours(4), session.RequesterKey);
            }
            await sessionLog.StartRequest($"{context.Request.PathBase}{context.Request.Path}");
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await handleError(context, sessionLog, ex);
            }
            finally
            {
                await sessionLog.EndRequest();
            }
        }

        private static bool isAnonSessionExpired(IAnonClient anonClient, Clock clock)
        {
            return !string.IsNullOrWhiteSpace(anonClient.SessionKey) && clock.Now().ToUniversalTime() > anonClient.SessionExpirationTime.ToUniversalTime();
        }

        private static void expireAnonSession(IAnonClient anonClient)
        {
            anonClient.Persist("", DateTimeOffset.MinValue, anonClient.RequesterKey);
        }

        private async Task handleError(HttpContext context, TempLogSession sessionLog, Exception ex)
        {
            await logException(sessionLog, ex);
            context.Response.StatusCode = getErrorStatusCode(ex);
            context.Response.ContentType = "application/json";
            var errors = new ResultContainer<ErrorModel[]>(getErrors(ex));
            var serializedErrors = JsonSerializer.Serialize(errors);
            await context.Response.WriteAsync(serializedErrors);
        }

        private static async Task logException(TempLogSession sessionLog, Exception ex)
        {
            AppEventSeverity severity;
            string caption;
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
            await sessionLog.LogException(severity, ex, caption);
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
