namespace XTI_WebAppClient
{
    public sealed class LoginModel
    {
        public LoginCredentials Credentials { get; set; }
        public string StartUrl { get; set; }
        public string ReturnUrl { get; set; }
    }
}
