﻿using FakeWebApp.Api;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XTI_App;
using XTI_Version;
using XTI_Version.Fakes;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.Tests
{
    public sealed class NewVersionCommandTest
    {
        [Test]
        public async Task ShouldCreateNewPatch()
        {
            var tester = await setup();
            tester.Options.NewVersion.Type = AppVersionType.Patch.DisplayText;
            var newVersion = await tester.Execute();
            Assert.That(newVersion?.IsPatch(), Is.True, "Should start new patch");
        }

        [Test]
        public async Task ShouldCreateNewMinorVersion()
        {
            var tester = await setup();
            tester.Options.NewVersion.Type = AppVersionType.Minor.DisplayText;
            var newVersion = await tester.Execute();
            Assert.That(newVersion?.IsMinor(), Is.True, "Should start new minor version");
        }

        [Test]
        public async Task ShouldCreateNewMajorVersion()
        {
            var tester = await setup();
            tester.Options.NewVersion.Type = AppVersionType.Major.DisplayText;
            var newVersion = await tester.Execute();
            Assert.That(newVersion?.IsMajor(), Is.True, "Should start new major version");
        }

        [Test]
        public async Task ShouldCreateMilestoneForNewVersion()
        {
            var tester = await setup();
            tester.Options.NewVersion.Type = AppVersionType.Major.DisplayText;
            var newVersion = await tester.Execute();
            var milestoneExists = await githubRepo(tester).MilestoneExists($"xti_major_version_{newVersion.ID}");
            Assert.That(milestoneExists, Is.True, "Should create milestone for new version");
        }

        [Test]
        public async Task ShouldCreateBranchForNewVersion()
        {
            var tester = await setup();
            tester.Options.NewVersion.Type = AppVersionType.Major.DisplayText;
            var newVersion = await tester.Execute();
            var branchExists = await githubRepo(tester).BranchExists($"xti/major/{newVersion.ID}");
            Assert.That(branchExists, Is.True, "Should create branch for new version");
        }

        private GitHubXtiRepoClient githubRepo(ManageVersionTester tester)
        {
            return tester.GithubClient.Repo(tester.Options.NewVersion.RepoOwner, tester.Options.NewVersion.RepoName);
        }

        private async Task<ManageVersionTester> setup()
        {
            var tester = new ManageVersionTester();
            await tester.Setup();
            tester.Options.Command = "New";
            return tester;
        }
    }
}