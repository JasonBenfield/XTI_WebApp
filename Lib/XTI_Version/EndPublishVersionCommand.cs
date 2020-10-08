using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XTI_App;

namespace XTI_Version
{
    public sealed class EndPublishVersionCommand
    {
        private readonly AppFactory factory;

        public EndPublishVersionCommand(AppFactory factory)
        {
            this.factory = factory;
        }

        public async Task<AppVersion> Execute(PublishVersionOptions options)
        {
            var xtiVersionBranch = new XtiVersionBranch(options.Branch);
            var version = await factory.Versions().Version(new AppVersionKey(xtiVersionBranch.VersionID()));
            await version.Published();
            return version;
        }
    }
}
