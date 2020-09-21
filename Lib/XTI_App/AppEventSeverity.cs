using System;

namespace XTI_App
{
    public sealed class AppEventSeverity : NumericValue, IEquatable<AppEventSeverity>
    {
        public static readonly AppEventSeverity NotSet = new AppEventSeverity(0, "Not Set");
        public static readonly AppEventSeverity CriticalError = new AppEventSeverity(100, "Critical Error");

        public static AppEventSeverity FromValue(int value) =>
            FromValue(new[] { NotSet, CriticalError }, value) ?? NotSet;

        private AppEventSeverity(int value, string displayText)
            : base(value, displayText)
        {
        }

        public bool Equals(AppEventSeverity other) => _Equals(other);
    }
}
