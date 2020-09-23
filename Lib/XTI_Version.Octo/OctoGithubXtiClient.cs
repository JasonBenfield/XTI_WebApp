using Microsoft.Extensions.Options;
using Octokit;

namespace XTI_Version.Octo
{
    public sealed class OctoGithubXtiClient : GitHubXtiClient
    {
        private GitHubClient client;
        private readonly GitHubOptions options;

        public OctoGithubXtiClient(IOptions<GitHubOptions> options)
        {
            this.options = options.Value;
        }

        public GitHubXtiRepoClient Repo(string owner, string name)
        {
            if (client == null)
            {
                client = new GitHubClient(new ProductHeaderValue("xti-app"));
                client.Credentials = new Credentials(options.UserName, options.Password);
            }
            return new OctoGithubXtiRepoClient(owner, name, client);
        }
    }
}
