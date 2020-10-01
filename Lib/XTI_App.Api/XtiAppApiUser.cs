using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XTI_App.Api
{
    public sealed class XtiAppApiUser : IAppApiUser
    {
        private readonly IAppContext appContext;
        private readonly ISessionContext userContext;

        public XtiAppApiUser(IAppContext appContext, ISessionContext userContext)
        {
            this.appContext = appContext;
            this.userContext = userContext;
        }

        public async Task<bool> HasAccess(ResourceAccess resourceAccess)
        {
            var app = await appContext.App();
            var roles = await app.Roles();
            var allowedRoles = resourceAccess.Allowed.Select(ar => roles.FirstOrDefault(r => r.Name().Equals(ar)));
            var user = await userContext.User();
            var userRoles = await user.RolesForApp(app);
            bool hasAccess = false;
            if (userRoles.Any(ur => allowedRoles.Any(ar => ur.IsRole(ar))))
            {
                hasAccess = true;
            }
            return hasAccess;
        }
    }
}
