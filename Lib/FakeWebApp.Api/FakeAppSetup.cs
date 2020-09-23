using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using XTI_App;

namespace FakeWebApp.Api
{
    public sealed class FakeAppSetup
    {
        private readonly AppFactory factory;
        private readonly Clock clock;

        public FakeAppSetup(IServiceProvider sp)
        {
            factory = sp.GetService<AppFactory>();
            clock = sp.GetService<Clock>();
        }

        public async Task Run()
        {
            await factory.AppRepository().AddApp(FakeAppApi.AppKey, clock.Now());
        }
    }
}
