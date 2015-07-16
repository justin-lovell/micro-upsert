using System;
using System.IO;

namespace MicroUpsert
{
    public sealed class DbCommandController
    {
        public DbCommandController(
            TextWriter bufferWriter,
            Func<StaticUpsertValue, string> generateUpsertParameter)
        {
            BufferWriter = bufferWriter;
            GenerateUpsertParameter = generateUpsertParameter;
        }

        public TextWriter BufferWriter { get; private set; }
        public Func<StaticUpsertValue, string> GenerateUpsertParameter { get; private set; }
    }
}
