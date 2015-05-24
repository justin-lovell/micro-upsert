namespace MicroUpsert
{
    public sealed class StaticUpsertValue : UpsertValue
    {
        public StaticUpsertValue(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }

        private bool Equals(StaticUpsertValue other)
        {
            return Equals(Value, other.Value);
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
            return obj is StaticUpsertValue && Equals((StaticUpsertValue) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Static Value: {0}", Value);
        }
    }
}