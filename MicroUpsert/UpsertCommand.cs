using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MicroUpsert
{
    public sealed class UpsertCommand
    {
        private readonly ReadOnlyCollection<UpsertVector> _values;

        public static UpsertCommand On(params UpsertVector[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                throw new ArgumentOutOfRangeException("values", "Zero-length array");
            }

            var upsertValues = (UpsertVector[])values.Clone();
            return new UpsertCommand(upsertValues);
        }

        private UpsertCommand(UpsertVector[] values)
        {
            _values = new ReadOnlyCollection<UpsertVector>(values);
        }

        public ReadOnlyCollection<UpsertVector> Values
        {
            get { return _values; }
        }

        private bool Equals(UpsertCommand other)
        {
            return _values.Count == other._values.Count
                   && _values.All(v => other._values.Contains(v));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj is UpsertCommand && Equals((UpsertCommand) obj);
        }

        public override int GetHashCode()
        {
            return (_values != null ? _values.GetHashCode() : 0);
        }
    }
}