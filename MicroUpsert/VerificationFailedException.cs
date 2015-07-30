using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MicroUpsert
{
    [Serializable]
    public class VerificationFailedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public VerificationFailedException()
        {
        }

        public VerificationFailedException(string message) : base(message)
        {
        }

        public VerificationFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected VerificationFailedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public VerificationFailedException(
            string message,
            KeyValuePair<KeyIdentity, UpsertCommand>[] unexpectedUpserts,
            KeyValuePair<KeyIdentity, UpsertCommand>[] missingUpserts,
            NonEqualUpsert[] nonEqualUpserts,
            KeyValuePair<string, CallProcedure>[] unexpectedProcedures,
            KeyValuePair<string, CallProcedure>[] missingProcedures,
            NonEqualProcedure[] nonEqualProcedures)
            : this(message)
        {
            UnexpectedUpserts = unexpectedUpserts;
            MissingUpserts = missingUpserts;
            NonEqualUpserts = nonEqualUpserts;
            UnexpectedProcedures = unexpectedProcedures;
            MissingProcedures = missingProcedures;
            NonEqualProcedures = nonEqualProcedures;
        }

        public KeyValuePair<KeyIdentity, UpsertCommand>[] UnexpectedUpserts { get; private set; }
        public KeyValuePair<KeyIdentity, UpsertCommand>[] MissingUpserts { get; private set; }
        public NonEqualUpsert[] NonEqualUpserts { get; private set; }
        public KeyValuePair<string, CallProcedure>[] UnexpectedProcedures { get; private set; }
        public KeyValuePair<string, CallProcedure>[] MissingProcedures { get; private set; }
        public NonEqualProcedure[] NonEqualProcedures { get; private set; }
    }
}
