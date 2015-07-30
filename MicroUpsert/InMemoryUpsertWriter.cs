using System;
using System.Collections.Generic;
using System.Data;

namespace MicroUpsert
{
    internal sealed class InMemoryUpsertWriter : UpsertWriter
    {
        public InMemoryUpsertWriter()
        {
            UpsertVectors = new List<Tuple<KeyIdentity, UpsertCommand>>();
            Procedures = new List<CallProcedure>();
        }

        public List<Tuple<KeyIdentity, UpsertCommand>> UpsertVectors { get; private set; }
        public List<CallProcedure> Procedures { get; private set; }

        public override void Upsert(KeyIdentity identity, UpsertCommand command)
        {
            UpsertVectors.Add(Tuple.Create(identity, command));
        }

        public override void Call(CallProcedure details)
        {
            Procedures.Add(details);
        }

        public override void Go()
        {
        }

        public override void GoAndRead(Action<IDataReader> readCallback)
        {
        }

        public void PurgeToWriter(UpsertWriter target)
        {
            foreach (var upsertVector in UpsertVectors)
            {
                target.Upsert(upsertVector.Item1, upsertVector.Item2);
            }

            foreach (var callProcedure in Procedures)
            {
                target.Call(callProcedure);
            }
        }
    }
}