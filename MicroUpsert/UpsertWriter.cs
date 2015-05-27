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

    // todo: DbDataExecutionPipeline

    // todo: SqlServerBuilder

    /*
     * INSERT INTO <table>
SELECT <natural keys>, <other stuff...>
WHERE NOT EXISTS
   -- no race condition here
   ( SELECT 1 FROM <table> WHERE <natural keys> )

IF @@ROWCOUNT = 0 BEGIN
  UPDATE ...
  WHERE <natural keys>
END*/
}
