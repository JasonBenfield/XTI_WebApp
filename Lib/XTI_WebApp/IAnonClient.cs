using System;

namespace XTI_WebApp
{
    public interface IAnonClient
    {
        string RequesterKey { get; }
        string SessionKey { get; }
        DateTime SessionExpirationTime { get; }

        void Load();
        void Persist(string sessionKey, DateTime sessionExpirationTime, string requesterKey);
    }
}