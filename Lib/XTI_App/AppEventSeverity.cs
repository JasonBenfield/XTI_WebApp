using System;
using System.Linq;

namespace XTI_App
{
    public sealed class AppEventSeverity : IEquatable<AppEventSeverity>, IEquatable<int>
    {
        public static readonly AppEventSeverity NotSet = new AppEventSeverity(0, "Not Set");
        public static readonly AppEventSeverity CriticalError = new AppEventSeverity(100, "Critical Error");

        public static AppEventSeverity FromValue(int value) =>
            new[] { NotSet, CriticalError }.First(e => e.Value == value);

        private AppEventSeverity(int severity, string displayText)
        {
            Value = severity;
            DisplayText = displayText;
        }

        public int Value { get; }
        public string DisplayText { get; }

        public override bool Equals(object obj)
        {
            if (obj is AppEventSeverity severity)
            {
                return Equals(severity);
            }
            if (obj is int value)
            {
                return Equals(value);
            }
            return base.Equals(obj);
        }

        public bool Equals(AppEventSeverity other) => Equals(other?.Value ?? 0);

        public bool Equals(int other) => Value == other;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{nameof(AppEventSeverity)} {Value}: {DisplayText}";
    }
}
