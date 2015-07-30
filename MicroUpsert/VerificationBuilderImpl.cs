using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroUpsert
{
    internal class VerificationBuilderImpl : IVerificationBuilder
    {
        private readonly Dictionary<string, CallProcedure> _actualProcedures;
        private readonly Dictionary<KeyIdentity, UpsertCommand> _actualUpserts;
        private readonly InMemoryUpsertWriter _writer = new InMemoryUpsertWriter();

        public VerificationBuilderImpl(
            List<Tuple<KeyIdentity, UpsertCommand>> upsertVectors,
            List<CallProcedure> procedures)
        {
            _actualUpserts = ListOfUpsertsToDictionary(upsertVectors);
            _actualProcedures = ListOfProceduresToDictionary(procedures);
        }

        public IVerificationBuilder MatchUpsert(KeyIdentity identity, UpsertCommand command)
        {
            _writer.Upsert(identity, command);
            return this;
        }

        public IVerificationBuilder MatchProcedure(CallProcedure details)
        {
            _writer.Call(details);
            return this;
        }

        public void Verify()
        {
            var collatedWriter = new InMemoryUpsertWriter();
            var bufferingWriter = new BufferingWindowUpsertWriter(collatedWriter);

            _writer.PurgeToWriter(bufferingWriter);
            bufferingWriter.Go();

            var expectedUpserts = ListOfUpsertsToDictionary(collatedWriter.UpsertVectors);
            var expectedProcedures = ListOfProceduresToDictionary(collatedWriter.Procedures);

            VerifyExpectations(expectedUpserts, expectedProcedures);
        }

        private static Dictionary<string, CallProcedure> ListOfProceduresToDictionary(List<CallProcedure> procedures)
        {
            return procedures.ToDictionary(t => t.ProcedureName);
        }

        private static Dictionary<KeyIdentity, UpsertCommand> ListOfUpsertsToDictionary(
            List<Tuple<KeyIdentity, UpsertCommand>> upsertVectors)
        {
            return upsertVectors.ToDictionary(t => t.Item1, t => t.Item2);
        }

        private void VerifyExpectations(
            Dictionary<KeyIdentity, UpsertCommand> expectedUpserts,
            Dictionary<string, CallProcedure> expectedProcedures)
        {
            var unexpectedUpserts =
                from actual in _actualUpserts
                where !expectedUpserts.ContainsKey(actual.Key)
                select actual.Value;

            var missingUpserts =
                from expected in expectedUpserts
                where !_actualUpserts.ContainsKey(expected.Key)
                select expected.Value;

            var unMatchedUpserts =
                from kp in _actualUpserts
                where expectedUpserts.ContainsKey(kp.Key)
                let actual = kp.Value
                let expected = expectedUpserts[kp.Key]
                where !actual.Equals(expected)
                select new NonEqualUpsert(kp.Key, actual, expected);


            var unexpectedProcedures =
                from actual in _actualProcedures
                where !expectedProcedures.ContainsKey(actual.Key)
                select actual.Value;

            var missingProcedures =
                from expected in expectedProcedures
                where !_actualProcedures.ContainsKey(expected.Key)
                select expected.Value;

            var unMatchedProcedures =
                from kp in _actualProcedures
                where expectedProcedures.ContainsKey(kp.Key)
                let actual = kp.Value
                let expected = expectedProcedures[kp.Key]
                where !actual.Equals(expected)
                select new NonEqualProcedure(kp.Key, actual, expected);

            var r = new
                    {
                        UnexpectedUpserts = unexpectedUpserts.ToArray(),
                        MissingUpserts = missingUpserts.ToArray(),
                        NonEqualUpserts = unMatchedUpserts.ToArray(),
                        UnexpectedProcedures = unexpectedProcedures.ToArray(),
                        MissingProcedures = missingProcedures.ToArray(),
                        NonEqualProcedures = unMatchedProcedures.ToArray()
                    };

            if (r.UnexpectedUpserts.Length == 0 && r.MissingUpserts.Length == 0 && r.NonEqualUpserts.Length == 0
                && r.UnexpectedProcedures.Length == 0 && r.MissingProcedures.Length == 0
                && r.NonEqualProcedures.Length == 0)
            {
                return;
            }

            var msg = "Verification failed. View exception details for more information";
            throw new VerificationFailedException(msg,
                                                  r.UnexpectedUpserts,
                                                  r.MissingUpserts,
                                                  r.NonEqualUpserts,
                                                  r.UnexpectedProcedures,
                                                  r.MissingProcedures,
                                                  r.NonEqualProcedures);
        }
    }
}
