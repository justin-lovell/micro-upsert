using System;
using System.IO;

namespace MicroUpsert
{
    public class DbCommandController
    {
        public DbCommandController(
            TextWriter bufferWriter,
            Func<StaticUpsertValue, string> insertParameter)
        {
            BufferWriter = bufferWriter;
            InsertParameter = insertParameter;
        }

        public TextWriter BufferWriter { get; private set; }
        public Func<StaticUpsertValue, string> InsertParameter { get; private set; }
    }
}