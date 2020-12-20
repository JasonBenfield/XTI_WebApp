﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XTI_WebAppClient
{
    public class NumericValues<T> where T : NumericValue
    {
        private static readonly Regex whitespaceRegex = new Regex("\\s+");

        private readonly List<T> values = new List<T>();

        protected NumericValues()
        {
        }

        protected T Add(T value)
        {
            values.Add(value);
            return value;
        }

        public T Value(int value) =>
            values.FirstOrDefault(nv => nv.Equals(value)) ?? values.FirstOrDefault();

        public T Value(string displayText)
        {
            return values
                .FirstOrDefault
                (
                    v => whitespaceRegex.Replace(v.DisplayText, "")
                        .Equals
                        (
                            whitespaceRegex.Replace(displayText, ""),
                            StringComparison.OrdinalIgnoreCase
                        )
                )
                ?? values.FirstOrDefault();
        }

        public T[] All() => values.ToArray();
    }
}
