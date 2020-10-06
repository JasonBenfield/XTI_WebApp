﻿using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.Tests
{
    public sealed class AuthorizationTest
    {
        [Test]
        public async Task ShouldHaveAccess_WhenUserBelongsToAnAllowedRole()
        {
            var input = await setup();
            var adminRole = await input.App.Role(FakeRoleNames.Instance.Admin);
            await input.User.AddRole(adminRole);
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.True, "Should have access when user belongs to an allowed role");
        }

        [Test]
        public async Task ShouldNotHaveAccess_WhenUserDoesNoBelongToAnAllowedRole()
        {
            var input = await setup();
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await input.User.AddRole(viewerRole);
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.False, "Should not have access when user does not belong to an allowed role");
        }

        [Test]
        public async Task ShouldNotHaveAccess_WhenUserBelongsToADeniedRole()
        {
            var input = await setup();
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await input.User.AddRole(viewerRole);
            var hasAccess = await input.Api.Product.AddProduct.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.False, "Should not have access when user belongs to a denied role");
        }

        [Test]
        public async Task ShouldNotHaveAccess_WhenUserBelongsToADeniedRoleEvenIfTheyBelongToAnAllowedRole()
        {
            var input = await setup();
            var adminRole = await input.App.Role(FakeRoleNames.Instance.Admin);
            await input.User.AddRole(adminRole);
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await input.User.AddRole(viewerRole);
            var hasAccess = await input.Api.Product.AddProduct.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.False, "Should not have access when user belongs to a denied role even if they belong to an allowed role");
        }

        [Test]
        public async Task ShouldHaveAccess_WhenUserBelongsToAnAllowedRole_ForModifiedAction()
        {
            var input = await setup();
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await input.User.AddRole(viewerRole, new AccessModifier("DifferentCompany"));
            var adminRole = await input.App.Role(FakeRoleNames.Instance.Admin);
            var modifier = new AccessModifier("MyCompany");
            await input.User.AddRole(adminRole, modifier);
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(modifier);
            Assert.That(hasAccess, Is.True, "Should have access when user belongs to an allowed role for modified action");
        }

        [Test]
        public async Task ShouldNotHaveAccess_WhenUserDoesNoBelongToAnAllowedRole_ForModifiedAction()
        {
            var input = await setup();
            var adminRole = await input.App.Role(FakeRoleNames.Instance.Admin);
            await input.User.AddRole(adminRole, new AccessModifier("DifferentCompany"));
            var modifier = new AccessModifier("MyCompany");
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await input.User.AddRole(viewerRole, modifier);
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(modifier);
            Assert.That(hasAccess, Is.False, "Should not have access when user does not belong to an allowed role for modified action");
        }

        [Test]
        public async Task ShouldHaveAccess_WhenUserBelongsToAnAllowedRoleWithDefaultModifier()
        {
            var input = await setup();
            var adminRole = await input.App.Role(FakeRoleNames.Instance.Admin);
            await input.User.AddRole(adminRole, AccessModifier.Default);
            var modifier = new AccessModifier("MyCompany");
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(modifier);
            Assert.That(hasAccess, Is.True, "Should have access when user belongs to an allowed role for modified action");
        }

        private async Task<TestInput> setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            var sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            var setup = new AppSetup(factory);
            await setup.Run();
            var clock = sp.GetService<FakeClock>();
            var app = await factory.Apps().AddApp(new AppKey("Fake"), clock.Now());
            await app.AddRole(FakeRoleNames.Instance.Admin);
            await app.AddRole(FakeRoleNames.Instance.Manager);
            await app.AddRole(FakeRoleNames.Instance.Viewer);
            var user = await factory.Users().Add
            (
                new AppUserName("someone"), new FakeHashedPassword("Password"), clock.Now()
            );
            var input = new TestInput(sp, app, user);
            var appContext = (FakeAppContext)sp.GetService<IAppContext>();
            appContext.SetApp(app);
            var sessionContext = (FakeUserContext)sp.GetService<IUserContext>();
            sessionContext.SetUser(user);
            return input;
        }

        private sealed class TestInput
        {
            public TestInput(ServiceProvider sp, App app, AppUser user)
            {
                Factory = sp.GetService<AppFactory>();
                Clock = sp.GetService<FakeClock>();
                App = app;
                User = user;
                Api = new FakeAppApi(sp.GetService<IAppApiUser>());
            }

            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public App App { get; }
            public AppUser User { get; }
            public FakeAppApi Api { get; }
        }
    }
}
