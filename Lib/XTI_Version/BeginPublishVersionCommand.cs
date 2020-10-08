using System.Threading.Tasks;
using XTI_App;

namespace XTI_Version
{
    public sealed class BeginPublishVersionCommand
    {
        private readonly AppFactory factory;

        public BeginPublishVersionCommand(AppFactory factory)
        {
            this.factory = factory;
        }

        public async Task<AppVersion> Execute(PublishVersionOptions options)
        {
            var xtiVersionBranch = new XtiVersionBranch(options.Branch);
            var versionID = xtiVersionBranch.VersionID();
            var version = await factory.Versions().Version(new AppVersionKey(versionID));
            if (!version.IsNew() && !version.IsPublishing())
            {
                throw new PublishVersionException($"Unable to begin publishing version {versionID} when it's status is not 'New' or 'Publishing'");
            }
            await version.Publishing();
            return version;
        }
    }
}
