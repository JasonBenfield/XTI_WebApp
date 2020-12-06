using System.Security.Claims;
using System.Security.Principal;
using XTI_App;

namespace XTI_WebApp.Fakes
{
    public sealed class FakeHttpUser
    {
        public ClaimsPrincipal Create() => new ClaimsPrincipal();

        public ClaimsPrincipal Create(string sessionKey, AppUser user)
        {
            var claims = new XtiClaimsCreator(sessionKey, user).Values();
            var identity = new ClaimsIdentity(claims, "Test");
            return new ClaimsPrincipal(identity);
        }
    }
}
