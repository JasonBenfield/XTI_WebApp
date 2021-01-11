using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class RedirectAppAction<TModel> : AppAction<TModel, WebRedirectResult>
    {
        private readonly string url;

        public RedirectAppAction(string url)
        {
            this.url = url;
        }

        public Task<WebRedirectResult> Execute(TModel _) => Task.FromResult(new WebRedirectResult(url));
    }
}
