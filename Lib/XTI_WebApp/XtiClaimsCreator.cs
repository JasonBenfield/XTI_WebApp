using System.Collections.Generic;
using System.Security.Claims;
using XTI_App.Abstractions;

namespace XTI_WebApp
{
    public sealed class XtiClaimsCreator
    {
        private readonly string sessionKey;
        private readonly IAppUser user;

        public XtiClaimsCreator(string sessionKey, IAppUser user)
        {
            this.sessionKey = sessionKey;
            this.user = user;
        }

        public IEnumerable<Claim> Values() => new[]
        {
            new Claim("UserID", user.ID.Value.ToString()),
            new Claim("SessionKey", sessionKey)
        };
    }
}
