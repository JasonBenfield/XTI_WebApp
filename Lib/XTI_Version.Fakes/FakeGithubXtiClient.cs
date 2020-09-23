using System.Collections.Generic;

namespace XTI_Version.Fakes
{
    public sealed class FakeGithubXtiClient : GitHubXtiClient
    {
        private readonly Dictionary<string, FakeGithubXtiRepoClient> repoLookup = new Dictionary<string, FakeGithubXtiRepoClient>();
        public GitHubXtiRepoClient Repo(string owner, string name)
        {
            var key = $"{owner}|{name}";
            if (!repoLookup.TryGetValue(key, out var client))
            {
                client = new FakeGithubXtiRepoClient(owner, name);
                repoLookup.Add(key, client);
            }
            return client;
        }
    }
}
