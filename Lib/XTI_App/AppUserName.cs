using System;

namespace XTI_App
{
    public sealed class AppUserName : IEquatable<AppUserName>, IEquatable<string>
    {
        public static readonly AppUserName Anon = new AppUserName("xti_anon");

        public AppUserName(string value)
        {
            this.value = value?.Trim().ToLower() ?? "";
            hashCode = this.value.GetHashCode();
        }

        private readonly string value;
        private readonly int hashCode;

        public string Value() => value;

        public override bool Equals(object obj)
        {
            if (obj is string str)
            {
                return Equals(str);
            }
            return Equals(obj as AppKey);
        }

        public override int GetHashCode() => hashCode;

        public bool Equals(AppUserName other) => Equals(other?.value);
        public bool Equals(string other) => value == other;

        public override string ToString() => $"{nameof(AppUserName)} {value}";
    }
}
