using System.Security.Claims;
using XTI_App.Abstractions;

namespace XTI_WebApp.Fakes
{
    public sealed class FakeHttpUser
    {
        public ClaimsPrincipal Create() => new ClaimsPrincipal();

        public ClaimsPrincipal Create(string sessionKey, IAppUser user)
        {
            var claims = new XtiClaimsCreator(sessionKey, user).Values();
            var identity = new ClaimsIdentity(claims, "Test");
            return new ClaimsPrincipal(identity);
        }
    }
}
