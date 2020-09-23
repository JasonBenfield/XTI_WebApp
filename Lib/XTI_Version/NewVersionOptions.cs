namespace XTI_Version
{
    public sealed class NewVersionOptions
    {
        public static readonly string NewVersion = "NewVersion";

        public string App { get; set; }
        public string Type { get; set; }
        public string RepoOwner { get; set; }
        public string RepoName { get; set; }
    }
}
