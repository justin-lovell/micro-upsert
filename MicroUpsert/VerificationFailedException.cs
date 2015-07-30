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
            UpsertCommand[] unexpectedUpserts,
            UpsertCommand[] missingUpserts,
            NonEqualUpsert[] nonEqualUpserts,
            CallProcedure[] unexpectedProcedures,
            CallProcedure[] missingProcedures,
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

        public UpsertCommand[] UnexpectedUpserts { get; private set; }
        public UpsertCommand[] MissingUpserts { get; private set; }
        public NonEqualUpsert[] NonEqualUpserts { get; private set; }
        public CallProcedure[] UnexpectedProcedures { get; private set; }
        public CallProcedure[] MissingProcedures { get; private set; }
        public NonEqualProcedure[] NonEqualProcedures { get; private set; }
    }
}
