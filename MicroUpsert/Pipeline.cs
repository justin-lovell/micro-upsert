using System;
using System.Collections;
using System.Data;

namespace MicroUpsert
{
    public abstract class Pipeline
    {
        public abstract void Upsert(KeyIdentity identity, UpsertCommand command);
        public abstract void Call(CallProcedure details);
        public abstract void Go();
        public abstract void GoAndRead(Action<IDataReader> readCallback);
    }


    // todo: BufferingWindowPipeline
    // todo: DbDataExecutionPipeline
    // todo: ListeningPipeline
    // todo: DevNullPipeline

    // todo: SqlServerBuilder
}
