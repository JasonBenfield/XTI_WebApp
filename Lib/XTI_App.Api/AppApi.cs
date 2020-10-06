using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XTI_App.Api
{
    public class AppApi
    {
        protected AppApi
        (
            string appKey,
            IAppApiUser user,
            ResourceAccess access = null
        )
        {
            Name = new XtiPath(appKey, "Current");
            this.user = user;
            Access = access ?? ResourceAccess.AllowAuthenticated();
        }

        private readonly IAppApiUser user;
        private readonly Dictionary<string, AppApiGroup> groups = new Dictionary<string, AppApiGroup>();

        public XtiPath Name { get; }

        public ResourceAccess Access { get; }

        public Task<bool> HasAccess() => user.HasAccessToApp();

        protected TGroup AddGroup<TGroup>(Func<IAppApiUser, TGroup> createGroup)
            where TGroup : AppApiGroup
        {
            var group = createGroup(user);
            groups.Add(group.Name.Group.ToLower(), group);
            return group;
        }

        public IEnumerable<AppApiGroup> Groups() => groups.Values.ToArray();

        public AppApiGroup Group(string groupName) => groups[groupName.ToLower()];

        public AppApiTemplate Template() => new AppApiTemplate(this);
    }
}
