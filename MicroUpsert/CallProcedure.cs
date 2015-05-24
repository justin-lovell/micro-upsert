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
    }
}
