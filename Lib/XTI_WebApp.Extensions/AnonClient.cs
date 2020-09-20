using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Text.Json;

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

        public void Load()
        {
            var cookieText = httpContextAccessor.HttpContext.Request.Cookies[cookieName];
            if (string.IsNullOrWhiteSpace(cookieText))
            {
                SessionID = 0;
            }
            else
            {
                var protectedBytes = Convert.FromBase64String(cookieText);
                var unprotectedBytes = protector.Unprotect(protectedBytes);
                var unprotectedText = UTF8Encoding.UTF8.GetString(unprotectedBytes);
                var info = JsonSerializer.Deserialize<AnonInfo>(unprotectedText);
                SessionID = info.SessionID;
            }
        }

        public void Persist(int sessionID)
        {
            var cookieText = JsonSerializer.Serialize(new AnonInfo { SessionID = sessionID });
            var unprotectedBytes = Encoding.Default.GetBytes(cookieText);
            var protectedBytes = protector.Protect(unprotectedBytes);
            var protectedText = Convert.ToBase64String(protectedBytes);
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
        }
    }
}
