using System;
using System.Collections.Generic;
using System.Data;

namespace MicroUpsert
{
    public class ListeningUpsertWriter : UpsertWriter
    {
        public ListeningUpsertWriter()
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
    }
}