using System;

namespace XTI_App
{
    public sealed class AppVersionStatus : NumericValue, IEquatable<AppVersionStatus>
    {
        public static readonly AppVersionStatus NotSet = new AppVersionStatus(0, "Not Set");
        public static readonly AppVersionStatus New = new AppVersionStatus(1, "New");
        public static readonly AppVersionStatus Publishing = new AppVersionStatus(2, "Publishing");
        public static readonly AppVersionStatus Old = new AppVersionStatus(3, "Old");
        public static readonly AppVersionStatus Current = new AppVersionStatus(4, "Current");

        public static AppVersionStatus FromValue(int value) =>
            FromValue(new[] { NotSet, New, Publishing, Old, Current }, value) ?? NotSet;

        private AppVersionStatus(int value, string displayText) : base(value, displayText)
        {
        }

        public bool Equals(AppVersionStatus other) => _Equals(other);
    }
}
