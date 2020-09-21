using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XTI_App
{
    public abstract class NumericValue : IEquatable<int>
    {
        protected static T FromValue<T>(IEnumerable<T> values, int value)
            where T : NumericValue
        {
            return (values ?? Enumerable.Empty<T>()).FirstOrDefault(v => v.Equals(value));
        }

        protected NumericValue(int value, string displayText)
        {
            Value = value;
            DisplayText = displayText;
        }

        public int Value { get; }
        public string DisplayText { get; }

        protected bool _Equals(NumericValue value) => Equals(value?.Value ?? -1);
        protected bool _EqualsAny(params int[] values) => values.Any(v => Equals(v));
        protected bool _EqualsAny(params NumericValue[] values) => values.Any(v => _Equals(v));

        public bool Equals(int other) => Value == other;

        public override bool Equals(object obj)
        {
            if (obj?.GetType() == GetType())
            {
                return Equals((NumericValue)obj);
            }
            if (obj is int value)
            {
                return Equals(value);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{GetType().Name} {Value}: {DisplayText}";
    }
}
