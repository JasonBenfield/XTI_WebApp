﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App.Abstractions;
using XTI_App.Api;

namespace XTI_WebApp.Api
{
    public sealed class PageContext : IPageContext
    {
        private readonly AppOptions appOptions;
        private readonly CacheBust cacheBust;
        private readonly IAppContext appContext;
        private readonly IUserContext userContext;
        private readonly IHostEnvironment hostEnvironment;

        public PageContext(IOptions<AppOptions> appOptions, CacheBust cacheBust, IAppContext appContext, IUserContext userContext, IHostEnvironment hostEnvironment)
        {
            this.appOptions = appOptions.Value;
            this.cacheBust = cacheBust;
            this.appContext = appContext;
            this.userContext = userContext;
            this.hostEnvironment = hostEnvironment;
        }

        public string BaseUrl { get; private set; } = "";
        public string CacheBust { get; private set; } = "";
        public string EnvironmentName { get; private set; } = "";
        public bool IsAuthenticated { get; private set; } = false;
        public string UserName { get; private set; } = "";
        public string AppTitle { get; private set; } = "";
        public string PageTitle { get; set; } = "";

        public async Task<string> Serialize()
        {
            BaseUrl = string.IsNullOrWhiteSpace(appOptions.BaseUrl) ? "/" : appOptions.BaseUrl;
            CacheBust = await cacheBust.Value();
            var app = await appContext.App();
            AppTitle = app.Title;
            EnvironmentName = hostEnvironment.EnvironmentName;
            var user = await userContext.User();
            if (user.UserName().Equals(AppUserName.Anon))
            {
                IsAuthenticated = false;
                UserName = "";
            }
            else
            {
                IsAuthenticated = true;
                UserName = user.UserName();
            }
            return JsonSerializer.Serialize(this);
        }
    }
}
