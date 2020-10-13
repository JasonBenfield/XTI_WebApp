using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_App.EF;
using XTI_WebApp.Extensions;
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
            var expectedRoleNames = FakeAppRoles.Instance.Values();
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
            var expectedRoleNames = FakeAppRoles.Instance.Values();
            Assert.That(cachedAppRoles.Select(ar => ar.Name()), Is.EquivalentTo(expectedRoleNames), "Should retrieve app roles from source");
        }

        private IServiceScope scope;

        private async Task<TestInput> setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            services.AddXtiContextServices();
            services.AddScoped(sp => XtiPath.Parse("/Fake/Current/Employees/Index"));
            services.AddScoped<FakeAppSetup>();
            var sp = services.BuildServiceProvider();
            scope = sp.CreateScope();
            var fakeSetup = scope.ServiceProvider.GetService<FakeAppSetup>();
            await fakeSetup.Run();
            return new TestInput(scope.ServiceProvider, fakeSetup.App);
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
                AppDbContext = sp.GetService<AppDbContext>();
                FakeApp = fakeApp;
                Services = sp;
            }
            public CachedAppContext CachedAppContext { get; }
            public AppDbContext AppDbContext { get; }
            public App FakeApp { get; }
            public IServiceProvider Services { get; }
        }
    }
}
