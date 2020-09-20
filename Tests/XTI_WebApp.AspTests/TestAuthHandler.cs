using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using XTI_App;
using XTI_WebApp.Extensions;

namespace XTI_WebApp.AspTests
{
    public sealed class TestAuthOptions
    {
        public bool IsEnabled { get; set; }
        public AppSession Session { get; set; }
        public AppUser User { get; set; }
    }

    public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, TestAuthOptions testOptions)
            : base(options, logger, encoder, clock)
        {
            this.testOptions = testOptions;
        }

        private readonly TestAuthOptions testOptions;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticateResult result;
            if (testOptions.IsEnabled)
            {
                var claims = new XtiClaimsCreator(testOptions.Session, testOptions.User).Values();
                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Test");
                result = AuthenticateResult.Success(ticket);
            }
            else
            {
                result = AuthenticateResult.NoResult();
            }
            return Task.FromResult(result);
        }
    }
}
