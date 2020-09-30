using Microsoft.Extensions.DependencyInjection;
using XTI_App.Api;

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
