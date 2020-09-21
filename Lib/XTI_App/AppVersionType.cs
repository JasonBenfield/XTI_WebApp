using System;
using System.Collections.Generic;
using System.Text;

namespace XTI_App
{
    public sealed class AppVersionType : NumericValue, IEquatable<AppVersionType>
    {
        public static readonly AppVersionType NotSet = new AppVersionType(0, "Not Set");
        public static readonly AppVersionType Major = new AppVersionType(1, "Major");
        public static readonly AppVersionType Minor = new AppVersionType(2, "Minor");
        public static readonly AppVersionType Patch = new AppVersionType(3, "Patch");

        public static AppVersionType FromValue(int value) =>
            FromValue(new[] { NotSet, Major, Minor, Patch }, value) ?? NotSet;

        private AppVersionType(int value, string displayText) : base(value, displayText)
        {
        }

        public bool Equals(AppVersionType other) => _Equals(other);
    }
}
