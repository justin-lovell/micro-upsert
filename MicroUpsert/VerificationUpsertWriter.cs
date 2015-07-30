using System;
using System.Data;

namespace MicroUpsert
{
    public class VerificationUpsertWriter : UpsertWriter
    {
        private readonly InMemoryUpsertWriter _writer = new InMemoryUpsertWriter();

        public override void Upsert(KeyIdentity identity, UpsertCommand command)
        {
            _writer.Upsert(identity, command);
        }

        public override void Call(CallProcedure details)
        {
            _writer.Call(details);
        }

        public override void Go()
        {
            _writer.Go();
        }

        public override void GoAndRead(Action<IDataReader> readCallback)
        {
            _writer.GoAndRead(readCallback);
        }

        public IVerificationBuilder StartVerification()
        {
            var collatedWriter = new InMemoryUpsertWriter();
            var bufferingWriter = new BufferingWindowUpsertWriter(collatedWriter);

            _writer.PurgeToWriter(bufferingWriter);

            return new VerificationBuilderImpl(collatedWriter.UpsertVectors, collatedWriter.Procedures);
        }
    }
}