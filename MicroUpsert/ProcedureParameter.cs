using System.Data;

namespace MicroUpsert
{
    public sealed class ProcedureParameter
    {
        public ProcedureParameter(string parameterName, DbType dbType, object value)
        {
            ParameterName = parameterName;
            DbType = dbType;
            Value = value;
        }

        public string ParameterName { get; private set; }
        public DbType DbType { get; private set; }
        public object Value { get; private set; }

        private bool Equals(ProcedureParameter other)
        {
            return string.Equals(ParameterName, other.ParameterName) && DbType == other.DbType
                   && Equals(Value, other.Value);
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
            return obj is ProcedureParameter && Equals((ProcedureParameter) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ParameterName != null ? ParameterName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) DbType;
                hashCode = (hashCode*397) ^ (Value != null ? Value.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
