using System.Collections.Generic;
using System.Security.Claims;
using XTI_App;

namespace XTI_WebApp
{
    public sealed class XtiClaimsCreator
    {
        private readonly AppSession session;
        private readonly IAppUser user;

        public XtiClaimsCreator(AppSession session, IAppUser user)
        {
            this.session = session;
            this.user = user;
        }

        public IEnumerable<Claim> Values() => new[]
        {
            new Claim("UserID", user.ID.ToString()),
            new Claim("SessionID", session.ID.ToString())
        };
    }
}
