﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Abstractions;
using XTI_App.Api;
using XTI_App.EfApi;
using XTI_Configuration.Extensions;
using XTI_WebApp.Api;
using XTI_WebApp.Fakes;
using XTI_WebApp.TestFakes;

namespace XTI_WebApp.Tests
{
    public sealed class CachedAppContextTest
    {
        [Test]
        public async Task ShouldRetrieveAppFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var app = await input.CachedAppContext.App();
            Assert.That(app.ID, Is.EqualTo(input.FakeApp.ID), "Should retrieve app from source");
            Assert.That(app.Title, Is.EqualTo(input.FakeApp.Title), "Should retrieve app from source");
        }

        [Test]
        public async Task ShouldRetrieveAppFromCache()
        {
            var input = await setup();
            setHttpContext(input);
            var app = await input.CachedAppContext.App();
            var originalTitle = app.Title;
            await input.FakeApp.SetTitle("New title");
            var cachedApp = await input.CachedAppContext.App();
            Assert.That(cachedApp.Title, Is.EqualTo(originalTitle), "Should retrieve app from cache");
        }

        [Test]
        public async Task ShouldRetrieveAppRolesFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var app = await input.CachedAppContext.App();
            var appRoles = await app.Roles();
            var expectedRoleNames = new[] { FakeInfo.Roles.Admin, FakeInfo.Roles.Viewer };
            Assert.That(appRoles.Select(ar => ar.Name()), Is.EquivalentTo(expectedRoleNames), "Should retrieve app roles from source");
        }

        [Test]
        public async Task ShouldRetrieveAppRolesFromCache()
        {
            var input = await setup();
            setHttpContext(input);
            var originalApp = await input.CachedAppContext.App();
            var originalAppRoles = await originalApp.Roles();
            await input.FakeApp.AddRole(new AppRoleName("New Role"));
            var cachedApp = await input.CachedAppContext.App();
            var cachedAppRoles = await cachedApp.Roles();
            var expectedRoleNames = new[] { FakeInfo.Roles.Admin, FakeInfo.Roles.Viewer };
            Assert.That(cachedAppRoles.Select(ar => ar.Name()), Is.EquivalentTo(expectedRoleNames), "Should retrieve app roles from source");
        }

        [Test]
        public async Task ShouldRetrieveAppVersionFromCache()
        {
            var input = await setup();
            setHttpContext(input);
            var originalApp = await input.CachedAppContext.App();
            var originalVersion = await originalApp.Version(AppVersionKey.Current);
            var newVersion = await input.FakeApp.StartNewMajorVersion(DateTime.UtcNow);
            await newVersion.Publishing();
            await newVersion.Published();
            var cachedApp = await input.CachedAppContext.App();
            var cachedVersion = await cachedApp.Version(AppVersionKey.Current);
            Assert.That(cachedVersion.ID, Is.EqualTo(originalVersion.ID), "Should retrieve current version from cache");
        }

        [Test]
        public async Task ShouldRetrieveResourceGroupFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var app = await input.CachedAppContext.App();
            var version = await app.Version(AppVersionKey.Current);
            var resourceGroupName = new ResourceGroupName("Employee");
            var resourceGroup = await version.ResourceGroup(resourceGroupName);
            Assert.That(resourceGroup.Name(), Is.EqualTo(resourceGroupName), "Should retrieve resource group from source");
        }

        [Test]
        public async Task ShouldRetrieveModifierCategoryFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var app = await input.CachedAppContext.App();
            var version = await app.Version(AppVersionKey.Current);
            var resourceGroup = await version.ResourceGroup(new ResourceGroupName("Employee"));
            var modCategory = await resourceGroup.ModCategory();
            Assert.That(modCategory.Name(), Is.EqualTo(new ModifierCategoryName("Department")), "Should retrieve modifier category from source");
        }

        [Test]
        public async Task ShouldRetrieveModifierCategoryFromCache()
        {
            var input = await setup();
            setHttpContext(input);
            var app = await input.CachedAppContext.App();
            var version = await app.Version(AppVersionKey.Current);
            var resourceGroup = await version.ResourceGroup(new ResourceGroupName("Employee"));
            await resourceGroup.ModCategory();
            var currentVersion = await input.FakeApp.CurrentVersion();
            var sourceResourceGroup = await currentVersion.ResourceGroup(new ResourceGroupName("Employee"));
            var unknownApp = await input.AppFactory.Apps().App(AppKey.Unknown);
            var defaultModCategory = await unknownApp.ModCategory(ModifierCategoryName.Default);
            await sourceResourceGroup.SetModCategory(defaultModCategory);
            var cachedModCategory = await resourceGroup.ModCategory();
            Assert.That(cachedModCategory.Name(), Is.EqualTo(new ModifierCategoryName("Department")), "Should retrieve modifier category from source");
        }

        [Test]
        public async Task ShouldRetrieveResourceFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var app = await input.CachedAppContext.App();
            var version = await app.Version(AppVersionKey.Current);
            var resourceGroup = await version.ResourceGroup(new ResourceGroupName("Employee"));
            var resource = await resourceGroup.Resource(new ResourceName("AddEmployee"));
            Assert.That(resource.Name(), Is.EqualTo(new ResourceName("AddEmployee")), "Should retrieve resource from source");
        }

        private async Task<TestInput> setup()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration
                (
                    (hostContext, config) =>
                    {
                        config.UseXtiConfiguration(hostContext.HostingEnvironment, new string[] { });
                    }
                )
                .ConfigureServices
                (
                    (hostContext, services) =>
                    {
                        services.AddFakesForXtiWebApp(hostContext.Configuration);
                        services.AddScoped<ISourceAppContext, DefaultAppContext>();
                        services.AddScoped<IAppContext>(sp =>
                        {
                            var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                            var cache = sp.GetService<IMemoryCache>();
                            var appContext = sp.GetService<ISourceAppContext>();
                            return new CachedAppContext(httpContextAccessor, cache, appContext);
                        });
                        services.AddSingleton(sp => FakeInfo.AppKey);
                        services.AddScoped(sp => XtiPath.Parse("/Fake/Current/Employees/Index"));
                        services.AddScoped<FakeAppSetup>();
                    }
                )
                .Build();
            var scope = host.Services.CreateScope();
            var sp = scope.ServiceProvider;
            var fakeSetup = sp.GetService<FakeAppSetup>();
            await fakeSetup.Run(AppVersionKey.Current);
            return new TestInput(sp, fakeSetup.App);
        }

        private void setHttpContext(TestInput input)
        {
            var httpContextAccessor = input.Services.GetService<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.RequestServices = input.Services;
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp, App fakeApp)
            {
                CachedAppContext = (CachedAppContext)sp.GetService<IAppContext>();
                AppFactory = sp.GetService<AppFactory>();
                FakeApp = fakeApp;
                Services = sp;
            }
            public CachedAppContext CachedAppContext { get; }
            public AppFactory AppFactory { get; }
            public App FakeApp { get; }
            public IServiceProvider Services { get; }
        }
    }
}
