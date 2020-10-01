using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            XtiPath xtiPath
        )
        {
            AppSession session;
            if (context.User.Identity.IsAuthenticated)
            {
                session = await authenticatedSession(context, appFactory);
            }
            else
            {
                session = await anonSession(context, clock, appFactory, anonClient);
            }
            var version = await retrieveVersion(appFactory, xtiPath);
            var request = await session.LogRequest(version, context.Request.Path, clock.Now());
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await handleError(context, clock, request, ex);
            }
        }

        private static Task<AppSession> authenticatedSession(HttpContext context, AppFactory appFactory)
        {
            var sessionRepo = appFactory.SessionRepository();
            var sessionIDClaim = context.User.Claims.First(c => c.Type == "SessionID");
            var sessionID = int.Parse(sessionIDClaim.Value);
            return sessionRepo.RetrieveByID(sessionID);
        }

        private static async Task<AppSession> anonSession(HttpContext context, Clock clock, AppFactory appFactory, AnonClient anonClient)
        {
            anonClient.Load();
            var sessionRepo = appFactory.SessionRepository();
            var session = await sessionRepo.RetrieveByID(anonClient.SessionID);
            if (!session.HasStarted() || session.HasEnded())
            {
                var userRepo = appFactory.UserRepository();
                var anonUser = await userRepo.RetrieveByUserName(AppUserName.Anon);
                var requesterKey = anonClient.RequesterKey;
                if (string.IsNullOrWhiteSpace(requesterKey))
                {
                    requesterKey = Guid.NewGuid().ToString("N");
                }
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var remoteAddress = context.Connection.RemoteIpAddress?.ToString();
                session = await sessionRepo.Create(anonUser, clock.Now(), requesterKey, userAgent, remoteAddress);
                anonClient.Persist(session.ID, requesterKey);
            }
            return session;
        }

        private static async Task<AppVersion> retrieveVersion(AppFactory appFactory, XtiPath xtiPath)
        {
            var app = await appFactory.AppRepository().App(new AppKey(xtiPath.App));
            AppVersion version;
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
