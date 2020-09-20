using System;
using System.Text.RegularExpressions;

namespace XTI_App
{
    public sealed class AppRoleName : IEquatable<AppRoleName>, IEquatable<string>
    {
        public AppRoleName(string displayText)
        {
            if (string.IsNullOrWhiteSpace(displayText))
            {
                throw new ArgumentException($"{nameof(displayText)} is required");
            }
            DisplayText = displayText.Trim();
            Value = whitespaceRegex.Replace(displayText?.Trim().ToLower() ?? "", "_");
            hashCode = Value.GetHashCode();
        }

        private static readonly Regex whitespaceRegex = new Regex("\\s+");

        private readonly int hashCode;

        public string Value { get; }
        public string DisplayText { get; }

        public override bool Equals(object obj)
        {
            if (obj is string str)
            {
                return Equals(str);
            }
            return Equals(obj as AppKey);
        }

        public override int GetHashCode() => hashCode;

        public bool Equals(AppRoleName other) => Equals(other?.Value);
        public bool Equals(string other) => Value == other;

        public override string ToString() => $"{nameof(AppRoleName)} {DisplayText}";
    }
}
