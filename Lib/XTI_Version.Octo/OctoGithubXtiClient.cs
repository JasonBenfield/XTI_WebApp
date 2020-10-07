using Microsoft.Extensions.Options;
using Octokit;
using System.Threading.Tasks;
using XTI_Secrets;

namespace XTI_Version.Octo
{
    public sealed class OctoGithubXtiClient : GitHubXtiClient
    {
        private GitHubClient client;
        private readonly SecretCredentialsFactory secretCredentialsFactory;

        public OctoGithubXtiClient(SecretCredentialsFactory secretCredentialsFactory)
        {
            this.secretCredentialsFactory = secretCredentialsFactory;
        }

        public async Task<GitHubXtiRepoClient> Repo(string owner, string name)
        {
            if (client == null)
            {
                client = new GitHubClient(new ProductHeaderValue("xti-app"));
                var secretCredentials = secretCredentialsFactory.Create("github");
                var credentials = await secretCredentials.Value();
                client.Credentials = new Credentials(credentials.UserName, credentials.Password);
            }
            return new OctoGithubXtiRepoClient(owner, name, client);
        }
    }
}
