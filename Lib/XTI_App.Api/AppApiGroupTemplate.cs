using System.Collections.Generic;
using System.Linq;

namespace XTI_App.Api
{
    public sealed class AppApiGroupTemplate
    {
        public AppApiGroupTemplate(string name, ResourceAccess access, IEnumerable<AppApiActionTemplate> actionTemplates)
        {
            Name = name;
            Access = access;
            ActionTemplates = actionTemplates;
        }

        public string Name { get; }
        public ResourceAccess Access { get; }
        public IEnumerable<AppApiActionTemplate> ActionTemplates { get; }

        public IEnumerable<ObjectValueTemplate> ObjectTemplates() =>
            ActionTemplates
                .SelectMany(a => a.ObjectTemplates())
                .Distinct();
    }
}
