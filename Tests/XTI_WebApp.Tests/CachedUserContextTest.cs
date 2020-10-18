using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_App.DB;
using XTI_App.Fakes;
using XTI_Core;
using XTI_WebApp.Api;
using XTI_WebApp.Fakes;
using XTI_WebApp.TestFakes;

namespace XTI_WebApp.Tests
{
    public sealed class CachedUserContextTest
    {
        [Test]
        public async Task ShouldRetrieveUserFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var user = await input.CachedUserContext.User();
            Assert.That(user.ID, Is.EqualTo(input.User.ID), "Should retrieve user from source");
            Assert.That(user.UserName(), Is.EqualTo(input.User.UserName()), "Should retrieve user from source");
        }

        [Test]
        public async Task ShouldRetrieveUserFromCache()
        {
            var input = await setup();
            setHttpContext(input);
            var originalUser = await input.CachedUserContext.User();
            var originalUserName = originalUser.UserName();
            var userRecord = await input.AppDbContext.Users.FirstAsync(u => u.ID == input.User.ID);
            input.AppDbContext.Users.Update(userRecord);
            userRecord.UserName = "changed.user";
            await input.AppDbContext.SaveChangesAsync();
            var cachedUser = await input.CachedUserContext.User();
            Assert.That(cachedUser.UserName, Is.EqualTo(originalUserName), "Should retrieve user from cache");
        }

        [Test]
        public async Task ShouldRetrieveUserRolesFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var user = await input.CachedUserContext.User();
            var userRoles = await user.RolesForApp(input.FakeApp);
            var viewerRole = await input.FakeApp.Role(FakeRoleNames.Instance.Viewer);
            Assert.That(userRoles.Select(ur => ur.RoleID), Is.EquivalentTo(new[] { viewerRole.ID }), "Should retrieve user roles from source");
        }

        [Test]
        public async Task ShouldRetrieveUserRolesFromCache()
        {
            var input = await setup();
            setHttpContext(input);
            var user = await input.CachedUserContext.User();
            var userRoles = await user.RolesForApp(input.FakeApp);
            var adminRole = await input.FakeApp.Role(FakeRoleNames.Instance.Admin);
            await input.User.AddRole(adminRole);
            var cachedUser = await input.CachedUserContext.User();
            var cachedUserRoles = await cachedUser.RolesForApp(input.FakeApp);
            var viewerRole = await input.FakeApp.Role(FakeRoleNames.Instance.Viewer);
            Assert.That(userRoles.Select(ur => ur.RoleID), Is.EquivalentTo(new[] { viewerRole.ID }), "Should retrieve user roles from source");
        }

        private async Task<TestInput> setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            services.AddXtiContextServices();
            services.AddScoped(sp => XtiPath.Parse("/Fake/Current/Employees/Index"));
            services.AddScoped<FakeAppSetup>();
            var sp = services.BuildServiceProvider();
            var fakeSetup = sp.GetService<FakeAppSetup>();
            await fakeSetup.Run();
            var appFactory = sp.GetService<AppFactory>();
            var clock = sp.GetService<Clock>();
            var user = await appFactory.Users().Add
            (
                new AppUserName("test.user"),
                new FakeHashedPassword("Testing12345"),
                clock.Now()
            );
            var viewerRole = await fakeSetup.App.Role(FakeRoleNames.Instance.Viewer);
            await user.AddRole(viewerRole);
            var input = new TestInput(sp, fakeSetup.App, user);
            await input.Session.StartSession();
            await input.Session.CurrentSession.Authenticate(user);
            return input;
        }

        private void setHttpContext(TestInput input)
        {
            var httpContextAccessor = input.Services.GetService<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.RequestServices = input.Services;
            var session = input.Session.CurrentSession;
            var claims = new XtiClaimsCreator(session, input.User).Values();
            httpContextAccessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp, App fakeApp, AppUser user)
            {
                CachedUserContext = (CachedUserContext)sp.GetService<IUserContext>();
                AppDbContext = sp.GetService<AppDbContext>();
                FakeApp = fakeApp;
                User = user;
                Session = sp.GetService<ISessionContext>();
                Services = sp;
            }
            public CachedUserContext CachedUserContext { get; }
            public AppDbContext AppDbContext { get; }
            public App FakeApp { get; }
            public AppUser User { get; }
            public ISessionContext Session { get; }
            public IServiceProvider Services { get; }
        }
    }
}
