using Microsoft.Extensions.DependencyInjection;
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
            await addRolesToUser(input, adminRole);
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.True, "Should have access when user belongs to an allowed role");
        }

        [Test]
        public async Task ShouldNotHaveAccess_WhenUserDoesNoBelongToAnAllowedRole()
        {
            var input = await setup();
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await addRolesToUser(input, viewerRole);
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.False, "Should not have access when user does not belong to an allowed role");
        }

        [Test]
        public async Task ShouldNotHaveAccess_WhenUserBelongsToADeniedRole()
        {
            var input = await setup();
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await addRolesToUser(input, viewerRole);
            var hasAccess = await input.Api.Product.AddProduct.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.False, "Should not have access when user belongs to a denied role");
        }

        [Test]
        public async Task ShouldNotHaveAccess_WhenUserBelongsToADeniedRoleEvenIfTheyBelongToAnAllowedRole()
        {
            var input = await setup();
            var adminRole = await input.App.Role(FakeRoleNames.Instance.Admin);
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await addRolesToUser(input, adminRole, viewerRole);
            var hasAccess = await input.Api.Product.AddProduct.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.False, "Should not have access when user belongs to a denied role even if they belong to an allowed role");
        }

        [Test]
        public async Task ShouldHaveAccess_WhenUserBelongsToAnAllowedRole_ForModifiedAction()
        {
            var input = await setup();
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await addRolesToUser(input, new AccessModifier("DifferentCompany"), viewerRole);
            var adminRole = await input.App.Role(FakeRoleNames.Instance.Admin);
            var modifier = new AccessModifier("MyCompany");
            await addRolesToUser(input, modifier, adminRole);
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(modifier);
            Assert.That(hasAccess, Is.True, "Should have access when user belongs to an allowed role for modified action");
        }

        [Test]
        public async Task ShouldNotHaveAccess_WhenUserDoesNoBelongToAnAllowedRole_ForModifiedAction()
        {
            var input = await setup();
            var adminRole = await input.App.Role(FakeRoleNames.Instance.Admin);
            await addRolesToUser(input, new AccessModifier("DifferentCompany"), adminRole);
            var modifier = new AccessModifier("MyCompany");
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await addRolesToUser(input, modifier, viewerRole);
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(modifier);
            Assert.That(hasAccess, Is.False, "Should not have access when user does not belong to an allowed role for modified action");
        }

        [Test]
        public async Task ShouldHaveAccess_WhenUserBelongsToAnAllowedRoleWithDefaultModifier()
        {
            var input = await setup();
            var adminRole = await input.App.Role(FakeRoleNames.Instance.Admin);
            await addRolesToUser(input, AccessModifier.Default, adminRole);
            var modifier = new AccessModifier("MyCompany");
            var hasAccess = await input.Api.Employee.AddEmployee.HasAccess(modifier);
            Assert.That(hasAccess, Is.True, "Should have access when user belongs to an allowed role for modified action");
        }

        [Test]
        public async Task AnonShouldNotHaveAccess_WhenAnonIsNotAllowed()
        {
            var input = await setup();
            var anonUser = await input.Factory.Users().User(AppUserName.Anon);
            input.UserContext.SetUser(anonUser);
            var hasAccess = await input.Api.Home.Index.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.False, "Anon should not have access unless anons are allowed");
        }

        [Test]
        public async Task AnonShouldHaveAccess_WhenAnonIsAllowed()
        {
            var input = await setup();
            var anonUser = await input.Factory.Users().User(AppUserName.Anon);
            input.UserContext.SetUser(anonUser);
            var hasAccess = await input.Api.Login.Index.HasAccess(AccessModifier.Default);
            Assert.That(hasAccess, Is.True, "Anon should have access when anons are allowed");
        }

        [Test]
        public async Task ShouldHaveAccessToApp_WhenTheUserBelongsToAnyAppRoles()
        {
            var input = await setup();
            var viewerRole = await input.App.Role(FakeRoleNames.Instance.Viewer);
            await addRolesToUser(input, viewerRole);
            var hasAccess = await input.Api.HasAccess();
            Assert.That(hasAccess, Is.True, "User should have access to app when they belong to any app roles");
        }

        [Test]
        public async Task ShouldNotHaveAccessToApp_WhenTheUserDoesNotBelongToAnyAppRoles()
        {
            var input = await setup();
            var hasAccess = await input.Api.HasAccess();
            Assert.That(hasAccess, Is.False, "User should not have access to app when they do not belong to any app roles");
        }

        private Task addRolesToUser(TestInput input, params AppRole[] roles) =>
            addRolesToUser(input, AccessModifier.Default, roles);

        private async Task addRolesToUser(TestInput input, AccessModifier modifier, params AppRole[] roles)
        {
            var user = await input.User();
            foreach (var role in roles)
            {
                await user.AddRole(role, modifier);
            }
        }

        private async Task<TestInput> setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            var sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            await new AppSetup(factory).Run();
            var clock = sp.GetService<FakeClock>();
            var app = await factory.Apps().AddApp(new AppKey("Fake"), "Fake", clock.Now());
            await app.AddRole(FakeRoleNames.Instance.Admin);
            await app.AddRole(FakeRoleNames.Instance.Manager);
            await app.AddRole(FakeRoleNames.Instance.Viewer);
            var user = await factory.Users().Add
            (
                new AppUserName("someone"), new FakeHashedPassword("Password"), clock.Now()
            );
            var input = new TestInput(sp, app);
            var appContext = (FakeAppContext)sp.GetService<IAppContext>();
            appContext.SetApp(app);
            input.UserContext.SetUser(user);
            return input;
        }

        private sealed class TestInput
        {
            public TestInput(ServiceProvider sp, App app)
            {
                Factory = sp.GetService<AppFactory>();
                Clock = sp.GetService<FakeClock>();
                UserContext = (FakeUserContext)sp.GetService<IUserContext>();
                App = app;
                Api = new FakeAppApi(sp.GetService<IAppApiUser>());
            }

            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public App App { get; }
            public FakeUserContext UserContext { get; }
            public FakeAppApi Api { get; }

            public async Task<AppUser> User() => (AppUser)await UserContext.User();
        }
    }
}
