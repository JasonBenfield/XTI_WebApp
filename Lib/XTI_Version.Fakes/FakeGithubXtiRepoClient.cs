using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_Version.Fakes
{
    public sealed class FakeGithubXtiRepoClient : GitHubXtiRepoClient
    {
        private readonly List<Branch> branches = new List<Branch>();
        private readonly List<Milestone> milestones = new List<Milestone>();

        public FakeGithubXtiRepoClient(string owner, string repoName) : base(owner, repoName)
        {
        }

        public Branch GetBranch(string repoOwner, string repoName, string name)
        {
            return branches.FirstOrDefault(b => b.RepoOwner == repoOwner && b.RepoName == repoName && b.Name == name);
        }

        protected override Task<bool> BranchExists(string repoOwner, string repoName, string name)
        {
            return Task.FromResult(GetBranch(repoOwner, repoName, name) != null);
        }

        protected override Task CreateBranch(string repoOwner, string repoName, string name)
        {
            var branch = new Branch
            {
                RepoOwner = repoOwner,
                RepoName = repoName,
                Name = name
            };
            branches.Add(branch);
            return Task.CompletedTask;
        }

        public Milestone GetMilestone(string repoOwner, string repoName, string name)
        {
            return milestones.FirstOrDefault(m => m.RepoOwner == repoOwner && m.RepoName == repoName && m.Name == name);
        }

        protected override Task<bool> MilestoneExists(string repoOwner, string repoName, string name)
        {
            return Task.FromResult(GetMilestone(repoOwner, repoName, name) != null);
        }

        protected override Task CreateMilestone(string repoOwner, string repoName, string name)
        {
            var mileston = new Milestone
            {
                RepoOwner = repoOwner,
                RepoName = repoName,
                Name = name
            };
            milestones.Add(mileston);
            return Task.CompletedTask;
        }

        public sealed class Branch
        {
            public string RepoOwner { get; set; }
            public string RepoName { get; set; }
            public string Name { get; set; }
        }

        public sealed class Milestone
        {
            public string RepoOwner { get; set; }
            public string RepoName { get; set; }
            public string Name { get; set; }
        }
    }
}
