using System;

namespace XTI_App
{
    public sealed class AppKey : IEquatable<AppKey>, IEquatable<string>
    {
        public static readonly AppKey Common = new AppKey("*");

        public AppKey(string value)
        {
            Value = value?.Trim().ToUpper() ?? "";
            hashCode = Value.GetHashCode();
        }

        private readonly int hashCode;

        public string Value { get; }

        public override bool Equals(object obj)
        {
            if (obj is string str)
            {
                return Equals(str);
            }
            return Equals(obj as AppKey);
        }

        public override int GetHashCode() => hashCode;

        public bool Equals(AppKey other) => Equals(other?.Value);
        public bool Equals(string other) => Value == other;

        public override string ToString() => $"{nameof(AppKey)} {Value}";
    }
}
