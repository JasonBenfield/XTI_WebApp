namespace XTI_Version
{
    public sealed class ManageVersionOptions
    {
        public static readonly string ManageVersion = "ManageVersion";

        public string Command { get; set; }
        public NewVersionOptions NewVersion { get; set; } = new NewVersionOptions();
        public PublishVersionOptions PublishVersion { get; set; } = new PublishVersionOptions();
    }
}
