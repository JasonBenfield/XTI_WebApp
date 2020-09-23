using System.Threading.Tasks;

namespace XTI_Version
{
    public abstract class GitHubXtiRepoClient
    {
        private readonly string repoOwner;
        private readonly string repoName;

        public GitHubXtiRepoClient(string owner, string repoName)
        {
            this.repoOwner = owner;
            this.repoName = repoName;
        }

        public Task<bool> BranchExists(string name) => BranchExists(repoOwner, repoName, name);
        protected abstract Task<bool> BranchExists(string repoOwner, string repoName, string name);

        public Task CreateBranch(string name) => CreateBranch(repoOwner, repoName, name);
        protected abstract Task CreateBranch(string repoOwner, string repoName, string name);

        public Task<bool> MilestoneExists(string name) => MilestoneExists(repoOwner, repoName, name);
        protected abstract Task<bool> MilestoneExists(string repoOwner, string repoName, string name);

        public Task CreateMilestone(string name) => CreateMilestone(repoOwner, repoName, name);
        protected abstract Task CreateMilestone(string repoOwner, string repoName, string name);
    }
}
