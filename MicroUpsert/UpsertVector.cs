using System;

namespace MicroUpsert
{
    public sealed class UpsertVector
    {
        private static readonly StringComparer InvariantCultureIgnoreCase = StringComparer.InvariantCultureIgnoreCase;

        public static UpsertVector WithValue(string columnName, object value)
        {
            return new UpsertVector(columnName, UpsertValue.Static(value));
        }

        private UpsertVector(string columnName, UpsertValue value)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException("columnName");
            }

            ColumnName = columnName;
            Value = value;
        }

        public string ColumnName { get; private set; }
        public UpsertValue Value { get; private set; }

        private bool Equals(UpsertVector other)
        {
            return InvariantCultureIgnoreCase.Equals(ColumnName, other.ColumnName)
                   && Value.Equals(other.Value);
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
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((UpsertVector) obj);
        }

        public override int GetHashCode()
        {
            return InvariantCultureIgnoreCase.GetHashCode(ColumnName).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("ColumnName: {0}, Value: {1}", ColumnName, Value);
        }
    }
}
