using System;

namespace XTI_WebApp.Fakes
{
    public sealed class FakeAnonClient : IAnonClient
    {
        public string RequesterKey { get; private set; }

        public string SessionKey { get; private set; }

        public DateTime SessionExpirationTime { get; private set; }

        public void Load()
        {
        }

        public void Persist(string sessionKey, DateTime sessionExpirationTime, string requesterKey)
        {
            SessionKey = sessionKey;
            SessionExpirationTime = sessionExpirationTime;
            RequesterKey = requesterKey;
        }
    }
}
