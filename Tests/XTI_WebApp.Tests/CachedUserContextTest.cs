using MainDB.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_App.Fakes;
using XTI_Configuration.Extensions;
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
            var userRecord = await input.MainDbContext.Users.FirstAsync(u => u.ID == input.User.ID.Value);
            input.MainDbContext.Users.Update(userRecord);
            userRecord.UserName = "changed.user";
            await input.MainDbContext.SaveChangesAsync();
            var cachedUser = await input.CachedUserContext.User();
            Assert.That(cachedUser.UserName, Is.EqualTo(originalUserName), "Should retrieve user from cache");
        }

        [Test]
        public async Task ShouldRefreshUser()
        {
            var input = await setup();
            setHttpContext(input);
            await input.CachedUserContext.User();
            var userRecord = await input.MainDbContext.Users.FirstAsync(u => u.ID == input.User.ID.Value);
            input.MainDbContext.Users.Update(userRecord);
            userRecord.UserName = "changed.user";
            await input.MainDbContext.SaveChangesAsync();
            var user = await input.AppFactory.Users().User(new AppUserName("changed.user"));
            input.CachedUserContext.RefreshUser(user);
            var cachedUser = await input.CachedUserContext.User();
            Assert.That(cachedUser.UserName(), Is.EqualTo(new AppUserName("changed.user")), "Should refresh user");
        }

        [Test]
        public async Task ShouldRetrieveUserRolesFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var user = await input.CachedUserContext.User();
            var userRoles = await user.RolesForApp(input.FakeApp);
            var viewerRole = await input.FakeApp.Role(FakeAppRoles.Instance.Viewer);
            Assert.That(userRoles.Select(ur => ur.RoleID), Is.EquivalentTo(new[] { viewerRole.ID.Value }), "Should retrieve user roles from source");
        }

        [Test]
        public async Task ShouldRetrieveUserRolesFromCache()
        {
            var input = await setup();
            setHttpContext(input);
            var user = await input.CachedUserContext.User();
            var userRoles = await user.RolesForApp(input.FakeApp);
            var adminRole = await input.FakeApp.Role(FakeAppRoles.Instance.Admin);
            await input.User.AddRole(adminRole);
            var cachedUser = await input.CachedUserContext.User();
            var cachedUserRoles = await cachedUser.RolesForApp(input.FakeApp);
            var viewerRole = await input.FakeApp.Role(FakeAppRoles.Instance.Viewer);
            Assert.That(userRoles.Select(ur => ur.RoleID), Is.EquivalentTo(new[] { viewerRole.ID.Value }), "Should retrieve user roles from source");
        }

        [Test]
        public async Task ShouldRetrieveIsModCategoryAdminFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var modCategory = await input.FakeApp.ModCategory(new ModifierCategoryName("Department"));
            await input.User.GrantFullAccessToModCategory(modCategory);
            var cachedUser = await input.CachedUserContext.User();
            var isAdmin = await cachedUser.IsModCategoryAdmin(modCategory);
            Assert.That(isAdmin, Is.True, "Should retrieve is mod category admin from source");
        }

        [Test]
        public async Task ShouldRetrieveIsModCategoryAdminFromCache()
        {
            var input = await setup();
            setHttpContext(input);
            var modCategory = await input.FakeApp.ModCategory(new ModifierCategoryName("Department"));
            await input.User.GrantFullAccessToModCategory(modCategory);
            var cachedUser = await input.CachedUserContext.User();
            await cachedUser.IsModCategoryAdmin(modCategory);
            await input.User.RevokeFullAccessToModCategory(modCategory);
            var cachedIsAdmin = await cachedUser.IsModCategoryAdmin(modCategory);
            Assert.That(cachedIsAdmin, Is.True, "Should retrieve is mod category admin from cache");
        }

        [Test]
        public async Task ShouldRetrieveHasModifierFromSource()
        {
            var input = await setup();
            setHttpContext(input);
            var modCategory = await input.FakeApp.ModCategory(new ModifierCategoryName("Department"));
            var modifier = await modCategory.AddOrUpdateModifier("IT", "IT");
            await input.User.AddModifier(modifier);
            var cachedUser = await input.CachedUserContext.User();
            var hasModifier = await cachedUser.HasModifier(modifier.ModKey());
            Assert.That(hasModifier, Is.True, "Should retrieve has modifier from source");
        }

        [Test]
        public async Task ShouldRetrieveHasModifierFromCache()
        {
            var input = await setup();
            setHttpContext(input);
            var modCategory = await input.FakeApp.ModCategory(new ModifierCategoryName("Department"));
            var modifier = await modCategory.AddOrUpdateModifier("IT", "IT");
            await input.User.AddModifier(modifier);
            var cachedUser = await input.CachedUserContext.User();
            await cachedUser.HasModifier(modifier.ModKey());
            await input.User.RemoveModifier(modifier);
            var cachedHasModifier = await cachedUser.HasModifier(modifier.ModKey());
            Assert.That(cachedHasModifier, Is.True, "Should retrieve has modifier from cache");
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
                        services.AddSingleton(sp => FakeAppKey.AppKey);
                        services.AddScoped(sp => XtiPath.Parse("/Fake/Current/Employees/Index"));
                        services.AddScoped<FakeAppSetup>();
                    }
                )
                .Build();
            var scope = host.Services.CreateScope();
            var sp = scope.ServiceProvider;
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
            var viewerRole = await fakeSetup.App.Role(FakeAppRoles.Instance.Viewer);
            await user.AddRole(viewerRole);
            var input = new TestInput(sp, fakeSetup.App, user);
            return input;
        }

        private void setHttpContext(TestInput input)
        {
            var httpContextAccessor = input.Services.GetService<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.RequestServices = input.Services;
            var sessionKey = Guid.NewGuid().ToString("N");
            var claims = new XtiClaimsCreator(sessionKey, input.User).Values();
            httpContextAccessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp, App fakeApp, AppUser user)
            {
                CachedUserContext = (CachedUserContext)sp.GetService<IUserContext>();
                MainDbContext = sp.GetService<MainDbContext>();
                FakeApp = fakeApp;
                User = user;
                AppFactory = sp.GetService<AppFactory>();
                Services = sp;
            }
            public CachedUserContext CachedUserContext { get; }
            public MainDbContext MainDbContext { get; }
            public AppFactory AppFactory { get; }
            public App FakeApp { get; }
            public AppUser User { get; }
            public IServiceProvider Services { get; }
        }
    }
}
