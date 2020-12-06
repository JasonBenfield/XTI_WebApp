using Microsoft.AspNetCore.Http;
using System.Linq;

namespace XTI_WebApp
{
    public sealed class XtiClaims
    {
        private readonly HttpContext httpContext;

        public XtiClaims(IHttpContextAccessor httpContextAccessor)
            : this(httpContextAccessor.HttpContext)
        {
        }

        public XtiClaims(HttpContext httpContext)
        {
            this.httpContext = httpContext;
        }

        public string SessionKey() => claim("SessionKey");

        public int UserID()
        {
            var userIDValue = claim("UserID");
            return string.IsNullOrWhiteSpace(userIDValue) ? 0 : int.Parse(userIDValue);
        }

        private string claim(string type)
        {
            var httpUser = httpContext?.User;
            return httpUser?.Identity.IsAuthenticated == true
                ? httpUser.Claims.First(c => c.Type == type).Value
                : "";
        }
    }
}
