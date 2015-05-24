using System;
using System.Collections.ObjectModel;

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
    }
}