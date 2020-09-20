using System;

namespace XTI_App
{
    public sealed class Timestamp : IEquatable<Timestamp>, IEquatable<DateTime>, IComparable<Timestamp>, IComparable<DateTime>
    {
        public static readonly Timestamp Empty = new Timestamp(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        public Timestamp(DateTime value)
        {
            Value = value;
        }

        public DateTime Value { get; }

        public bool IsEmpty() => CompareTo(Empty) <= 0;

        public int CompareTo(Timestamp other) => Value.CompareTo(other.Value);

        public int CompareTo(DateTime other) => Value.CompareTo(other);

        public override bool Equals(object obj)
        {
            if (obj is Timestamp timestamp)
            {
                return Equals(timestamp);
            }
            if (obj is DateTime dateTime)
            {
                return Equals(dateTime);
            }
            return base.Equals(obj);
        }

        public bool Equals(DateTime other) => Value == other;

        public bool Equals(Timestamp other) => Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }
}
