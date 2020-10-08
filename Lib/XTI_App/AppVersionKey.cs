using System;
using System.Text.RegularExpressions;

namespace XTI_App
{
    public sealed class AppVersionKey : SemanticType<string>, IEquatable<AppVersionKey>
    {
        private static readonly Regex keyRegex = new Regex("V(\\d+)");

        public static readonly AppVersionKey None = new AppVersionKey(0);

        public static AppVersionKey Parse(string str)
        {
            AppVersionKey key;
            if (string.IsNullOrWhiteSpace(str))
            {
                key = None;
            }
            else
            {
                if (!keyRegex.IsMatch(str))
                {
                    throw new ArgumentException($"'{str}' is not a valid version key");
                }
                key = new AppVersionKey(str);
            }
            return key;
        }

        public AppVersionKey(int versionID) : this($"V{versionID}")
        {
        }

        private AppVersionKey(string key) : base(key)
        {
        }

        public int VersionNumber()
        {
            var match = keyRegex.Match(Value);
            return int.Parse(match.Groups[1].Value);
        }

        public bool Equals(AppVersionKey other) => _Equals(other);
    }
}
