using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using XTI_Secrets;

namespace XTI_WebApp.Extensions
{
    public sealed class AnonClient
    {
        private readonly IDataProtector protector;
        private readonly IHttpContextAccessor httpContextAccessor;

        private static readonly string cookieName = "xti_anon";

        public AnonClient(IDataProtector protector, IHttpContextAccessor httpContextAccessor)
        {
            this.protector = protector;
            this.httpContextAccessor = httpContextAccessor;
        }

        public int SessionID { get; private set; }
        public string RequesterKey { get; private set; }

        public void Load()
        {
            var cookieText = httpContextAccessor.HttpContext.Request.Cookies[cookieName];
            if (string.IsNullOrWhiteSpace(cookieText))
            {
                SessionID = 0;
            }
            else
            {
                var unprotectedText = new DecryptedValue(protector, cookieText).Value();
                var info = JsonSerializer.Deserialize<AnonInfo>(unprotectedText);
                SessionID = info.SessionID;
                RequesterKey = info.RequesterKey;
            }
        }

        public void Persist(int sessionID, string requesterKey)
        {
            var cookieText = JsonSerializer.Serialize(new AnonInfo
            {
                SessionID = sessionID,
                RequesterKey = requesterKey
            });
            var protectedText = new EncryptedValue(protector, cookieText).Value();
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.Lax
            };
            httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, protectedText, options);
        }

        private class AnonInfo
        {
            public int SessionID { get; set; }
            public string RequesterKey { get; set; }
        }
    }
}
