using System.Threading.Tasks;

namespace XTI_Version
{
    public interface GitHubXtiClient
    {
        Task<GitHubXtiRepoClient> Repo(string owner, string name);
    }
}
