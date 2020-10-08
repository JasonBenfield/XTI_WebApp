namespace XTI_WebApp.Extensions
{
    public interface IAnonClient
    {
        string RequesterKey { get; }
        int SessionID { get; }

        void Load();
        void Persist(int sessionID, string requesterKey);
    }
}