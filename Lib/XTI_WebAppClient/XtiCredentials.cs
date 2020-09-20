namespace XTI_WebAppClient
{
    public sealed class XtiCredentials
    {
        public XtiCredentials(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public string UserName { get; }
        public string Password { get; }
    }
}
