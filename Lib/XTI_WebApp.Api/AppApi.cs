using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XTI_App;

namespace XTI_WebApp.Api
{
    public class AppApi
    {
        protected AppApi
        (
            string appKey,
            WebAppUser user,
            ResourceAccess access = null
        )
        {
            Name = new XtiPath(appKey, "");
            this.user = user;
            Access = access ?? ResourceAccess.AllowAnonymous();
        }

        private readonly WebAppUser user;
        private readonly Dictionary<string, AppApiGroup> groups = new Dictionary<string, AppApiGroup>();

        public XtiPath Name { get; }

        public ResourceAccess Access { get; }

        public Task<bool> HasAccess() => user.HasAccess(Access);

        public async Task EnsureUserHasAccess()
        {
            var hasAccess = await HasAccess();
            if (!hasAccess)
            {
                throw new AccessDeniedException(Name);
            }
        }

        protected TGroup AddGroup<TGroup>(Func<WebAppUser, TGroup> createGroup)
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
