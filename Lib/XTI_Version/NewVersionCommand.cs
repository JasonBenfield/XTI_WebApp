using System.Threading.Tasks;
using System.Xml.Linq;
using XTI_App;

namespace XTI_Version
{
    public sealed class NewVersionCommand
    {
        private readonly AppFactory factory;
        private readonly Clock clock;
        private readonly GitHubXtiClient githubClient;

        public NewVersionCommand(AppFactory factory, Clock clock, GitHubXtiClient githubClient)
        {
            this.factory = factory;
            this.clock = clock;
            this.githubClient = githubClient;
        }

        public async Task<AppVersion> Execute(NewVersionOptions options)
        {
            var app = await factory.AppRepository().App(new AppKey(options.App));
            AppVersion version;
            var versionType = AppVersionType.Parse(options.Type);
            if (versionType.Equals(AppVersionType.Major))
            {
                version = await app.StartNewMajorVersion(clock.Now());
            }
            else if (versionType.Equals(AppVersionType.Minor))
            {
                version = await app.StartNewMinorVersion(clock.Now());
            }
            else if (versionType.Equals(AppVersionType.Patch))
            {
                version = await app.StartNewPatch(clock.Now());
            }
            else
            {
                version = null;
            }
            var gitHubRepo = githubClient.Repo(options.RepoOwner, options.RepoName);
            await createMilestoneIfNoneExists(gitHubRepo, version);
            await createBranchIfNoneExists(gitHubRepo, version);
            return version;
        }

        private async Task createMilestoneIfNoneExists(GitHubXtiRepoClient repo, AppVersion version)
        {
            var milestoneName = $"xti_major_version_{version.ID}";
            var exists = await repo.MilestoneExists(milestoneName);
            if (!exists)
            {
                await repo.CreateMilestone(milestoneName);
            }
        }

        private async Task createBranchIfNoneExists(GitHubXtiRepoClient repo, AppVersion version)
        {
            var branchName = $"xti/major/{version.ID}";
            var exists = await repo.BranchExists(branchName);
            if (!exists)
            {
                await repo.CreateBranch(branchName);
            }
        }
    }
}
