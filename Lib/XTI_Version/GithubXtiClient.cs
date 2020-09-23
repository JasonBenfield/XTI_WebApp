namespace XTI_Version
{
    public interface GitHubXtiClient
    {
        GitHubXtiRepoClient Repo(string owner, string name);
    }
}
