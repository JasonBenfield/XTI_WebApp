using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using XTI_Core.Extensions;

namespace XTI_WebApp.Extensions
{
    public sealed class TerminateMiddleware
    {
        private readonly RequestDelegate _next;

        public TerminateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IWebHostEnvironment hostEnvironment, IHostApplicationLifetime lifetime)
        {
            if (hostEnvironment.IsTest() && context.Request.Path.Value.Equals("/StopApp"))
            {
                lifetime.StopApplication();
            }
            else
            {
                await _next(context);
            }
        }
    }
}
