using System;
using System.Data;

namespace MicroUpsert
{
    public abstract class UpsertWriter
    {
        public abstract void Upsert(KeyIdentity identity, UpsertCommand command);
        public abstract void Call(CallProcedure details);
        public abstract void Go();
        public abstract void GoAndRead(Action<IDataReader> readCallback);
    }
}
