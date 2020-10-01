using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_WebApp.Extensions
{
    public sealed class WebAppContext : IAppContext
    {
        private readonly XtiPath xtiPath;
        private readonly AppFactory appFactory;

        public WebAppContext(XtiPath xtiPath, AppFactory appFactory)
        {
            this.xtiPath = xtiPath;
            this.appFactory = appFactory;
        }

        public async Task<IApp> App() =>
            await appFactory.AppRepository().App(new AppKey(xtiPath.App));
    }
}
