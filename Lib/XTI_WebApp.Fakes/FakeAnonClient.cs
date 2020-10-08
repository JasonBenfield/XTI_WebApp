using XTI_WebApp.Extensions;

namespace XTI_WebApp.Fakes
{
    public sealed class FakeAnonClient : IAnonClient
    {
        public string RequesterKey { get; private set; }

        public int SessionID { get; private set; }

        public void Load()
        {
        }

        public void Persist(int sessionID, string requesterKey)
        {
            RequesterKey = requesterKey;
            SessionID = sessionID;
        }
    }
}
