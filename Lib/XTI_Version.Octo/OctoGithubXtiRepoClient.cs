using Octokit;
using Octokit.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_Version.Octo
{
    public sealed class OctoGithubXtiRepoClient : GitHubXtiRepoClient
    {
        private readonly GitHubClient client;

        public OctoGithubXtiRepoClient(string owner, string repoName, GitHubClient client)
            : base(owner, repoName)
        {
            this.client = client;
        }

        protected override async Task<bool> BranchExists(string repoOwner, string repoName, string name)
        {
            Reference branch;
            try
            {
                branch = await client.Git.Reference.Get(repoOwner, repoName, $"refs/heads/{name}");
            }
            catch (NotFoundException)
            {
                branch = null;
            }
            return branch != null;
        }

        protected override Task CreateBranch(string repoOwner, string repoName, string name)
        {
            return client.Git.Reference.CreateBranch(repoOwner, repoName, name);
        }

        protected override Task CreateMilestone(string repoOwner, string repoName, string name)
        {
            var milestone = new NewMilestone(name);
            return client.Issue.Milestone.Create(repoOwner, repoName, milestone);
        }

        protected override async Task<bool> MilestoneExists(string repoOwner, string repoName, string name)
        {
            var milestones = await client.Issue.Milestone.GetAllForRepository(repoOwner, repoName, new MilestoneRequest
            {
                State = ItemStateFilter.Open
            });
            return milestones.Any(m => m.Title == name);
        }
    }
}
