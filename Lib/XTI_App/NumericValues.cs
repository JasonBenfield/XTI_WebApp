using System.Collections.Generic;
using System.Linq;

namespace XTI_App
{
    public class NumericValues<T> where T : NumericValue
    {
        private readonly List<T> values = new List<T>();

        protected NumericValues(T defaultValue)
        {
            DefaultValue = defaultValue;
        }

        protected T DefaultValue { get; }

        protected T Add(T value)
        {
            values.Add(value);
            return value;
        }

        public T Value(int value) =>
            values.FirstOrDefault(nv => nv.Equals(value)) ?? DefaultValue;

        public T[] All() => values.ToArray();
    }
}
