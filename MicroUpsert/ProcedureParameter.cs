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
        public ParameterDirection Direction { get; set; }

        public ProcedureParameter WithDirection(ParameterDirection direction)
        {
            Direction = direction;
            return this;
        }
    }
}
