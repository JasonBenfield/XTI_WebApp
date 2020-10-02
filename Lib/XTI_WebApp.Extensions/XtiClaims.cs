using Microsoft.AspNetCore.Http;
using System.Linq;

namespace XTI_WebApp.Extensions
{
    public sealed class XtiClaims
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public XtiClaims(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public int SessionID()
        {
            var sessionIDValue = claim("SessionID");
            return string.IsNullOrWhiteSpace(sessionIDValue) ? 0 : int.Parse(sessionIDValue);
        }

        public int UserID()
        {
            var userIDValue = claim("UserID");
            return string.IsNullOrWhiteSpace(userIDValue) ? 0 : int.Parse(userIDValue);
        }

        private string claim(string type) =>
            httpContextAccessor.HttpContext.User.Identity.IsAuthenticated
                ? httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == type).Value
                : "";
    }
}
