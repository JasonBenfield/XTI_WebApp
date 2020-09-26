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
            var appRepo = factory.AppRepository();
            var fakeApp = await appRepo.App(FakeAppApi.AppKey);
            if (!fakeApp.Exists())
            {
                await appRepo.AddApp(FakeAppApi.AppKey, clock.Now());
            }
        }
    }
}
