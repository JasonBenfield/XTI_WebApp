using System;
using System.Threading.Tasks;
using XTI_App;

namespace XTI_Version
{
    public sealed class ManageVersionCommand
    {
        private readonly NewVersionCommand newVersionCommand;
        private readonly BeginPublishVersionCommand beginPublishVersionCommand;
        private readonly EndPublishVersionCommand endPublishVersionCommand;

        public ManageVersionCommand(NewVersionCommand newVersionCommand, BeginPublishVersionCommand beginPublishVersionCommand, EndPublishVersionCommand endPublishVersionCommand)
        {
            this.newVersionCommand = newVersionCommand;
            this.beginPublishVersionCommand = beginPublishVersionCommand;
            this.endPublishVersionCommand = endPublishVersionCommand;
        }

        public async Task<AppVersion> Execute(ManageVersionOptions options)
        {
            AppVersion version;
            if (options.Command.Equals("New", StringComparison.OrdinalIgnoreCase))
            {
                version = await newVersionCommand.Execute(options.NewVersion);
            }
            else if (options.Command.Equals("BeginPublish", StringComparison.OrdinalIgnoreCase))
            {
                version = await beginPublishVersionCommand.Execute(options.PublishVersion);
            }
            else if (options.Command.Equals("EndPublish", StringComparison.OrdinalIgnoreCase))
            {
                version = await endPublishVersionCommand.Execute(options.PublishVersion);
            }
            else
            {
                version = null;
            }
            return version;
        }
    }
}
