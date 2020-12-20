﻿using System;
using System.Linq;

namespace XTI_WebAppClient
{
    public abstract class NumericValue : IEquatable<int>
    {
        protected NumericValue(int value, string displayText)
        {
            Value = value;
            DisplayText = displayText;
        }

        public int Value { get; set; }
        public string DisplayText { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is int val)
            {
                return Equals(val);
            }
            if (GetType() == obj?.GetType())
            {
                return _Equals((NumericValue)obj);
            }
            return base.Equals(obj);
        }
        public override int GetHashCode() => Value.GetHashCode();
        public bool Equals(int other) => Value == other;
        protected bool _Equals(NumericValue other) => other != null && Equals(other.Value);
        protected bool _EqualsAny(params int[] values) => values.Any(v => Equals(v));
        protected bool _EqualsAny(params NumericValue[] values) => values.Any(v => _Equals(v));

        public override string ToString() => $"{GetType().Name} {Value}: {DisplayText}";
    }
}
