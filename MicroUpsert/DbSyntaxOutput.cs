using System;
using System.IO;

namespace MicroUpsert
{
    public class DbSyntaxOutput
    {
        public DbSyntaxOutput(
            TextWriter bufferWriter,
            Func<string> generateUniqueParameterName,
            Action<string, StaticUpsertValue> insertParameter)
        {
            BufferWriter = bufferWriter;
            GenerateUniqueParameterName = generateUniqueParameterName;
            InsertParameter = insertParameter;
        }

        public TextWriter BufferWriter { get; private set; }
        public Func<string> GenerateUniqueParameterName { get; private set; }
        public Action<string, StaticUpsertValue> InsertParameter { get; private set; }
    }
}