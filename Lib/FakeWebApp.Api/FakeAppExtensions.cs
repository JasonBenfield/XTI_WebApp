using Microsoft.Extensions.DependencyInjection;
using XTI_WebApp.Api;

namespace FakeWebApp.Api
{
    public static class FakeAppExtensions
    {
        public static void AddServicesForFakeWebApp(this IServiceCollection services)
        {
            services.AddSingleton<AppApi, FakeAppApi>();
        }
    }
}
