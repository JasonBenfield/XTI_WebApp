﻿using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using XTI_App;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.Tests
{
    public sealed class AppVersionTest
    {
        [Test]
        public async Task ShouldStartNewVersionForApp()
        {
            var input = await setup();
            var version = await input.App.StartNewPatch(input.Clock.Now());
            var versions = (await input.App.Versions()).ToArray();
            Assert.That(versions.Length, Is.EqualTo(1), "Should add version to app");
            Assert.That(versions[0].ID, Is.EqualTo(version.ID));
        }

        [Test]
        public async Task ShouldSetStatusToPublishing()
        {
            var input = await setup();
            var version = await input.App.StartNewPatch(input.Clock.Now());
            await version.Publishing();
            Assert.That(version.IsPublishing(), Is.True, "Should set status to publishing");
        }

        [Test]
        public async Task ShouldSetStatusToCurrent_WhenPublished()
        {
            var input = await setup();
            var version = await input.App.StartNewPatch(input.Clock.Now());
            await version.Publishing();
            await version.Published();
            Assert.That(version.IsCurrent(), Is.True, "Should set status to current when published");
        }

        [Test]
        public async Task ShouldOnlyAllowOneCurrentVersion()
        {
            var input = await setup();
            var originalCurrent = await input.App.StartNewPatch(input.Clock.Now());
            await originalCurrent.Publishing();
            await originalCurrent.Published();
            var current = await input.App.StartNewPatch(input.Clock.Now());
            await current.Publishing();
            await current.Published();
            Assert.That(originalCurrent.IsCurrent(), Is.False, "Should only allow one current version");
        }

        [Test]
        public async Task ShouldAssignVersionNumber_WhenPatchBeginsPublishing()
        {
            var input = await setup();
            var patch = await input.App.StartNewPatch(input.Clock.Now());
            await patch.Publishing();
            Assert.That(patch.Major, Is.EqualTo(0), "Should assign version number for new patch");
            Assert.That(patch.Minor, Is.EqualTo(0), "Should assign version number for new patch");
            Assert.That(patch.Patch, Is.EqualTo(1), "Should assign version number for new patch");
        }

        [Test]
        public async Task ShouldAssignVersionNumber_WhenMinorVersionBeginsPublishing()
        {
            var input = await setup();
            var minorVersion = await input.App.StartNewMinorVersion(input.Clock.Now());
            await minorVersion.Publishing();
            Assert.That(minorVersion.Major, Is.EqualTo(0), "Should assign version number for new minor version");
            Assert.That(minorVersion.Minor, Is.EqualTo(1), "Should assign version number for new minor version");
            Assert.That(minorVersion.Patch, Is.EqualTo(0), "Should assign version number for new minor version");
        }

        [Test]
        public async Task ShouldAssignVersionNumber_WhenMajorVersionBeginsPublishing()
        {
            var input = await setup();
            var majorVersion = await input.App.StartNewMajorVersion(input.Clock.Now());
            await majorVersion.Publishing();
            Assert.That(majorVersion.Major, Is.EqualTo(1), "Should assign version number for new major version");
            Assert.That(majorVersion.Minor, Is.EqualTo(0), "Should assign version number for new major version");
            Assert.That(majorVersion.Patch, Is.EqualTo(0), "Should assign version number for new major version");
        }

        [Test]
        public async Task ShouldIncrementPatchOfCurrent_WhenPatchBeginsPublishing()
        {
            var input = await setup();
            var originalCurrent = await input.App.StartNewPatch(input.Clock.Now());
            await originalCurrent.Publishing();
            await originalCurrent.Published();
            var patch = await input.App.StartNewPatch(input.Clock.Now());
            await patch.Publishing();
            Assert.That(patch.Major, Is.EqualTo(0), "Should increment patch of current version");
            Assert.That(patch.Minor, Is.EqualTo(0), "Should increment patch of current version");
            Assert.That(patch.Patch, Is.EqualTo(2), "Should increment patch of current version");
        }

        [Test]
        public async Task ShouldIncrementMinorVersionOfCurrent_WhenMinorVersionBeginsPublishing()
        {
            var input = await setup();
            var originalCurrent = await input.App.StartNewMinorVersion(input.Clock.Now());
            await originalCurrent.Publishing();
            await originalCurrent.Published();
            var minorVersion = await input.App.StartNewMinorVersion(input.Clock.Now());
            await minorVersion.Publishing();
            Assert.That(minorVersion.Major, Is.EqualTo(0), "Should increment minor of current version");
            Assert.That(minorVersion.Minor, Is.EqualTo(2), "Should increment minor of current version");
            Assert.That(minorVersion.Patch, Is.EqualTo(0), "Should increment minor of current version");
        }

        [Test]
        public async Task ShouldIncrementMajorVersionOfCurrent_WhenMajorVersionBeginsPublishing()
        {
            var input = await setup();
            var originalCurrent = await input.App.StartNewMajorVersion(input.Clock.Now());
            await originalCurrent.Publishing();
            await originalCurrent.Published();
            var majorVersion = await input.App.StartNewMajorVersion(input.Clock.Now());
            await majorVersion.Publishing();
            Assert.That(majorVersion.Major, Is.EqualTo(2), "Should increment major of current version");
            Assert.That(majorVersion.Minor, Is.EqualTo(0), "Should increment major of current version");
            Assert.That(majorVersion.Patch, Is.EqualTo(0), "Should increment major of current version");
        }

        [Test]
        public async Task ShouldRetainMajorVersion_WhenMinorVersionBeginsPublishing()
        {
            var input = await setup();
            var majorVersion = await input.App.StartNewMajorVersion(input.Clock.Now());
            await majorVersion.Publishing();
            await majorVersion.Published();
            var minorVersion = await input.App.StartNewMinorVersion(input.Clock.Now());
            await minorVersion.Publishing();
            await minorVersion.Published();
            Assert.That(minorVersion.Major, Is.EqualTo(1), "Should retain major version from the previous current");
            Assert.That(minorVersion.Minor, Is.EqualTo(1), "Should increment minor version");
            Assert.That(minorVersion.Patch, Is.EqualTo(0), "Should reset patch");
        }

        [Test]
        public async Task ShouldRetainMajorAndMinorVersion_WhenPatchBeginsPublishing()
        {
            var input = await setup();
            var majorVersion = await input.App.StartNewMajorVersion(input.Clock.Now());
            await majorVersion.Publishing();
            await majorVersion.Published();
            var minorVersion = await input.App.StartNewMinorVersion(input.Clock.Now());
            await minorVersion.Publishing();
            await minorVersion.Published();
            var patch = await input.App.StartNewPatch(input.Clock.Now());
            await patch.Publishing();
            Assert.That(patch.Major, Is.EqualTo(1), "Should retain major version from the previous current");
            Assert.That(patch.Minor, Is.EqualTo(1), "Should retain minor version from the previous current");
            Assert.That(patch.Patch, Is.EqualTo(1), "Should increment patch");
        }

        [Test]
        public async Task ShouldResetPatch_WhenMinorVersionBeginsPublishing()
        {
            var input = await setup();
            var patch = await input.App.StartNewPatch(input.Clock.Now());
            await patch.Publishing();
            await patch.Published();
            var minorVersion = await input.App.StartNewMinorVersion(input.Clock.Now());
            await minorVersion.Publishing();
            Assert.That(minorVersion.Major, Is.EqualTo(0), "Should reset patch when minor version is publishing");
            Assert.That(minorVersion.Minor, Is.EqualTo(1), "Should reset patch when minor version is publishing");
            Assert.That(minorVersion.Patch, Is.EqualTo(0), "Should reset patch when minor version is publishing");
        }

        [Test]
        public async Task ShouldResetPatchAndMinorVersion_WhenMajorVersionBeginsPublishing()
        {
            var input = await setup();
            var patch = await input.App.StartNewPatch(input.Clock.Now());
            await patch.Publishing();
            await patch.Published();
            var minorVersion = await input.App.StartNewMinorVersion(input.Clock.Now());
            await minorVersion.Publishing();
            await minorVersion.Published();
            var majorVersion = await input.App.StartNewMajorVersion(input.Clock.Now());
            await majorVersion.Publishing();
            Assert.That(majorVersion.Major, Is.EqualTo(1), "Should reset minor version and patch when major version is publishing");
            Assert.That(majorVersion.Minor, Is.EqualTo(0), "Should reset minor version and patch when major version is publishing");
            Assert.That(majorVersion.Patch, Is.EqualTo(0), "Should reset minor version and patch when major version is publishing");
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
            return new TestInput(factory, clock, app);
        }

        private sealed class TestInput
        {
            public TestInput(AppFactory factory, FakeClock clock, App app)
            {
                Factory = factory;
                Clock = clock;
                App = app;
            }

            public AppFactory Factory { get; }
            public FakeClock Clock { get; }
            public App App { get; }
        }
    }
}
