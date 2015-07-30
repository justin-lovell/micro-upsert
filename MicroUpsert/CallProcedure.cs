using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroUpsert
{
    public sealed class CallProcedure
    {
        private readonly ProcedureParameter[] _parameters;

        private CallProcedure(string procedureName, params ProcedureParameter[] parameters)
        {
            if (procedureName == null)
            {
                throw new ArgumentNullException("procedureName");
            }

            ProcedureName = procedureName;
            _parameters = parameters != null
                              ? parameters.Select(_ => _).ToArray()
                              : new ProcedureParameter[0];
        }

        public string ProcedureName { get; private set; }

        public IEnumerable<ProcedureParameter> Parameters
        {
            get { return _parameters; }
        }

        public static CallProcedure Create(string procedureName, params ProcedureParameter[] parameters)
        {
            return new CallProcedure(procedureName, parameters);
        }

        private bool Equals(CallProcedure other)
        {
            return _parameters.Length == other._parameters.Length
                   && _parameters.All(p => other._parameters.Contains(p))
                   && string.Equals(ProcedureName, other.ProcedureName);
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
            return obj is CallProcedure && Equals((CallProcedure) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_parameters != null ? _parameters.GetHashCode() : 0)*397)
                       ^ (ProcedureName != null ? ProcedureName.GetHashCode() : 0);
            }
        }
    }
}
