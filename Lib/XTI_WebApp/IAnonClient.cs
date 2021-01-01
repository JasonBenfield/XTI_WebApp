using System;

namespace XTI_WebApp
{
    public interface IAnonClient
    {
        string RequesterKey { get; }
        string SessionKey { get; }
        DateTimeOffset SessionExpirationTime { get; }

        void Load();
        void Persist(string sessionKey, DateTimeOffset sessionExpirationTime, string requesterKey);
    }
}