using System;
using System.Collections.Generic;
using System.Linq;

namespace XTI_App
{
    public sealed class XtiPath : IEquatable<XtiPath>, IEquatable<string>
    {
        public static readonly string CurrentVersion = "Current";

        public static XtiPath Parse(string str)
        {
            var parts = (str ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries);
            var names = new List<string>(parts.Concat(Enumerable.Repeat("", 4 - parts.Length)));
            return new XtiPath(names[0], names[1], names[2], names[3]);
        }

        public XtiPath(string appKey, string version) : this(appKey, version, null, null)
        {
        }

        public XtiPath(string appKey, string version, string group) : this(appKey, version, group, null)
        {
        }

        public XtiPath(string appKey, string version, string group, string action)
        {
            if (string.IsNullOrWhiteSpace(appKey) && (!string.IsNullOrWhiteSpace(group) || !string.IsNullOrWhiteSpace(action))) { throw new ArgumentException($"{nameof(appKey)} is required"); }
            if (string.IsNullOrWhiteSpace(group) && !string.IsNullOrWhiteSpace(action)) { throw new ArgumentException($"{nameof(group)} is required when there is an action"); }
            App = appKey;
            Version = string.IsNullOrWhiteSpace(version) ? CurrentVersion : version;
            Group = group;
            Action = action;
            value = $"{App}/{Version}/{Group}/{Action}".ToLower();
            hashCode = value.GetHashCode();
        }

        private readonly string value;
        private readonly int hashCode;

        public string App { get; }
        public string Version { get; }
        public string Group { get; }
        public string Action { get; }

        public bool IsCurrentVersion() => Version == CurrentVersion;
        public int VersionID() => IsCurrentVersion() ? 0 : int.Parse(Version.Substring(1));

        public void EnsureAppResource()
        {
            if (!string.IsNullOrWhiteSpace(Group))
            {
                throw new ArgumentException($"{Format()} is not the name of an app");
            }
        }

        public void EnsureGroupResource()
        {
            if (string.IsNullOrWhiteSpace(Group) || !string.IsNullOrWhiteSpace(Action))
            {
                throw new ArgumentException($"{Format()} is not the name of a group");
            }
        }

        public void EnsureActionResource()
        {
            if (string.IsNullOrWhiteSpace(Action))
            {
                throw new ArgumentException($"{Format()} is not the name of an action");
            }
        }

        public XtiPath WithGroup(string groupName)
        {
            if (!string.IsNullOrWhiteSpace(Group)) { throw new ArgumentException("Cannot create group for a group"); }
            return new XtiPath(App, Version, groupName);
        }

        public XtiPath WithAction(string actionName)
        {
            if (!string.IsNullOrWhiteSpace(Action)) { throw new ArgumentException("Cannot create action for an action"); }
            return new XtiPath(App, Version, Group, actionName);
        }

        public string Format()
        {
            var parts = new string[] { App, Version, Group, Action }.TakeWhile(str => !string.IsNullOrWhiteSpace(str));
            return string.Join("/", parts);
        }

        public string Value() => Format().ToLower();

        public override bool Equals(object obj)
        {
            if (obj is string str)
            {
                return Equals(str);
            }
            return Equals(obj as XtiPath);
        }

        public bool Equals(XtiPath other) => value == other?.value;

        public bool Equals(string other) => value == other;

        public override int GetHashCode() => hashCode;

        public override string ToString()
        {
            var str = string.IsNullOrWhiteSpace(App) ? "Empty" : Format();
            return $"{nameof(XtiPath)} {str}";
        }

    }
}
