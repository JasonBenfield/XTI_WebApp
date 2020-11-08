using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_WebApp.Fakes;
using XTI_WebApp.TestFakes;

namespace XTI_WebApp.Tests
{
    public sealed class SessionLogTest
    {
        [Test]
        public async Task ShouldGetSessionByKey()
        {
            var input = await setup();
            var key = generateKey();
            var createdSession = await createSession(input, key);
            var foundSession = await input.Factory.Sessions().Session(key);
            Assert.That(foundSession.ID, Is.EqualTo(createdSession.ID), "Should get session by key");
        }

        [Test]
        public async Task ShouldGetRequestByKey()
        {
            var input = await setup();
            var session = await createSession(input, generateKey());
            var key = generateKey();
            var createdRequest = await createRequest(input, session, key);
            var foundRequest = await input.Factory.Requests().Request(key);
            Assert.That(foundRequest.ID, Is.EqualTo(createdRequest.ID), "Should get request by key");
        }

        [Test]
        public async Task ShouldAddEvent()
        {
            var input = await setup();
            var session = await createSession(input, generateKey());
            var request = await createRequest(input, session, generateKey());
            var key = generateKey();
            var createdEvent = await request.LogEvent(key, AppEventSeverity.Values.CriticalError, DateTime.Now, "Test", "Testing", "Testing Details");
            Assert.That(createdEvent.Severity, Is.EqualTo(AppEventSeverity.Values.CriticalError), "Should add event");
        }

        private async Task<AppRequest> createRequest(TestInput input, AppSession session, string key)
        {
            var currentVersion = await input.FakeApp.CurrentVersion();
            var action = input.Api.Employee.AddEmployee;
            var resourceGroup = await input.FakeApp.ResourceGroup(action.Path.Group);
            var resource = await resourceGroup.Resource(action.Path.Action);
            var createdRequest = await session.LogRequest(key, currentVersion, resource, null, action.Path.Value(), DateTime.Now);
            return createdRequest;
        }

        [Test]
        public async Task ShouldGetEventByKey()
        {
            var input = await setup();
            var createdSession = await createSession(input, generateKey());
            var key = generateKey();
            var currentVersion = await input.FakeApp.CurrentVersion();
            var action = input.Api.Employee.AddEmployee;
            var resourceGroup = await input.FakeApp.ResourceGroup(action.Path.Group);
            var resource = await resourceGroup.Resource(action.Path.Action);
            var createdRequest = await createdSession.LogRequest(key, currentVersion, resource, null, action.Path.Value(), DateTime.Now);
            var foundRequest = await input.Factory.Requests().Request(key);
            Assert.That(foundRequest.ID, Is.EqualTo(createdRequest.ID), "Should get request by key");
        }

        private static async Task<AppSession> createSession(TestInput input, string key)
        {
            var user = await input.Factory.Users().User(AppUserName.Anon);
            var createdSession = await input.Factory.Sessions().Create(key, user, DateTime.Now, "RequesterKey", "UserAgent", "my-computer");
            return createdSession;
        }

        private string generateKey() => Guid.NewGuid().ToString("N");

        private async Task<TestInput> setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            services.AddFakeXtiContexts();
            services.AddSingleton(sp => FakeAppKey.AppKey);
            services.AddScoped
            (
                sp => new FakeAppApi
                (
                    FakeAppKey.AppKey,
                    AppVersionKey.Current,
                    sp.GetService<IAppApiUser>()
                )
            );
            var sp = services.BuildServiceProvider();
            var factory = sp.GetService<AppFactory>();
            var clock = sp.GetService<Clock>();
            var setup = new FakeAppSetup(factory, clock);
            await setup.Run();
            var app = await factory.Apps().App(FakeAppKey.AppKey);
            return new TestInput(sp, app);
        }

        private sealed class TestInput
        {
            public TestInput(ServiceProvider sp, App app)
            {
                Factory = sp.GetService<AppFactory>();
                Clock = sp.GetService<FakeClock>();
                Api = sp.GetService<FakeAppApi>();
                FakeApp = app;
            }

            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public FakeAppApi Api { get; }
            public App FakeApp { get; }
        }
    }
}
