﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App;
using XTI_WebApp.Api;

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
            AnonClient anonClient
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
            var resourceName = new AppResourceName(context.Request.Path);
            var request = await session.AddRequest(resourceName, clock.Now());
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await request.LogCriticalException(clock.Now(), ex, "An unexpected error occurred");
                context.Response.StatusCode = getErrorStatusCode(ex);
                context.Response.ContentType = "application/json";
                var errors = getErrors(ex);
                var serializedErrors = JsonSerializer.Serialize(errors);
                await context.Response.WriteAsync(serializedErrors);
            }
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
            if (session.IsNotFound() || session.HasEnded())
            {
                var userRepo = appFactory.UserRepository();
                var anonUser = await userRepo.RetrieveByUserName(AppUserName.Anon);
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var remoteAddress = context.Connection.RemoteIpAddress?.ToString() ?? "";
                session = await sessionRepo.Create(anonUser, clock.Now(), userAgent, remoteAddress);
                anonClient.Persist(session.ID);
            }
            return session;
        }
    }
}
