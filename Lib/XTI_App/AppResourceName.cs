using System;
using System.Collections.Generic;
using System.Linq;

namespace XTI_App
{
    public sealed class AppResourceName : IEquatable<AppResourceName>, IEquatable<string>
    {
        public static AppResourceName Parse(string str)
        {
            var parts = (str ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries);
            var names = new List<string>(parts.Concat(Enumerable.Repeat("", 3 - parts.Length)));
            return new AppResourceName(names[0], names[1], names[2]);
        }

        public AppResourceName(string appKey) : this(appKey, null, null)
        {
        }

        public AppResourceName(string appKey, string group) : this(appKey, group, null)
        {
        }

        public AppResourceName(string appKey, string group, string action)
        {
            if (string.IsNullOrWhiteSpace(appKey) && (!string.IsNullOrWhiteSpace(group) || !string.IsNullOrWhiteSpace(action))) { throw new ArgumentException($"{nameof(appKey)} is required"); }
            if (string.IsNullOrWhiteSpace(group) && !string.IsNullOrWhiteSpace(action)) { throw new ArgumentException($"{nameof(group)} is required when there is an action"); }
            App = appKey;
            Group = group;
            Action = action;
            value = $"{App}/{Group}/{Action}".ToLower();
            hashCode = value.GetHashCode();
        }

        private readonly string value;
        private readonly int hashCode;

        public string App { get; }
        public string Group { get; }
        public string Action { get; }

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

        public AppResourceName WithGroup(string groupName)
        {
            if (!string.IsNullOrWhiteSpace(Group)) { throw new ArgumentException("Cannot create group for a group"); }
            return new AppResourceName(App, groupName);
        }

        public AppResourceName WithAction(string actionName)
        {
            if (!string.IsNullOrWhiteSpace(Action)) { throw new ArgumentException("Cannot create action for an action"); }
            return new AppResourceName(App, Group, actionName);
        }

        public string Format()
        {
            var parts = new string[] { App, Group, Action }.TakeWhile(str => !string.IsNullOrWhiteSpace(str));
            return string.Join("/", parts);
        }

        public string Value() => Format().ToLower();

        public override bool Equals(object obj)
        {
            if (obj is string str)
            {
                return Equals(str);
            }
            return Equals(obj as AppResourceName);
        }

        public bool Equals(AppResourceName other) => value == other?.value;

        public bool Equals(string other) => value == other;

        public override int GetHashCode() => hashCode;

        public override string ToString()
        {
            var str = string.IsNullOrWhiteSpace(App) ? "Empty" : Format();
            return $"{nameof(AppResourceName)} {str}";
        }

    }
}
