using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XTI_App
{
    public abstract class NumericValue : SemanticType<int>, IEquatable<int>
    {
        protected static T FromValue<T>(IEnumerable<T> values, int value)
            where T : NumericValue
        {
            return (values ?? Enumerable.Empty<T>()).FirstOrDefault(v => v.Equals(value));
        }

        protected static T FromDisplayText<T>(IEnumerable<T> values, string displayText)
            where T : NumericValue
        {
            return (values ?? Enumerable.Empty<T>()).FirstOrDefault(v => v.DisplayText.Equals(displayText, StringComparison.OrdinalIgnoreCase));
        }

        protected NumericValue(int value, string displayText)
            : base(value, displayText)
        {
        }

        protected bool _EqualsAny(params int[] values) => values.Any(v => Equals(v));
        protected bool _EqualsAny(params NumericValue[] values) => values.Any(v => _Equals(v));

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => $"{GetType().Name} {Value}: {DisplayText}";
    }
}
