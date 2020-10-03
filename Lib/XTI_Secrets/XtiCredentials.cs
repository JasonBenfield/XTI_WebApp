using System;

namespace XTI_Secrets
{
    public sealed class XtiCredentials : IEquatable<XtiCredentials>
    {
        public XtiCredentials(string userName, string password)
        {
            UserName = userName;
            Password = password;
            hashCode = $"{UserName}|{Password}".GetHashCode();
        }

        private readonly int hashCode;

        public string UserName { get; }
        public string Password { get; }

        public bool Equals(XtiCredentials other)
        {
            return UserName == other.UserName && Password == other.Password;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XtiCredentials);
        }

        public override int GetHashCode() => hashCode;

        public override string ToString() => $"{nameof(XtiCredentials)} {UserName}";

    }
}
