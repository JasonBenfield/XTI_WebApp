using System;

namespace XTI_App
{
    public sealed class AppEventSeverity : NumericValue, IEquatable<AppEventSeverity>
    {
        public sealed class AppEventSeverities : NumericValues<AppEventSeverity>
        {
            public AppEventSeverities()
                : base(new AppEventSeverity(0, "Not Set"))
            {
                NotSet = DefaultValue;
                CriticalError = add(100, "Critical Error");
            }
            private AppEventSeverity add(int value, string displayText) =>
                Add(new AppEventSeverity(value, displayText));
            public AppEventSeverity NotSet { get; }
            public AppEventSeverity CriticalError { get; }
        }

        public static readonly AppEventSeverities Values = new AppEventSeverities();

        public static AppEventSeverity FromValue(int value) => Values.Value(value);

        private AppEventSeverity(int value, string displayText)
            : base(value, displayText)
        {
        }

        public bool Equals(AppEventSeverity other) => _Equals(other);
    }
}
