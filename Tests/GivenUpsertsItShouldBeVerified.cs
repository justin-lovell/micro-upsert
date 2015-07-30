using System.Data;
using NUnit.Framework;

namespace MicroUpsert
{
    [TestFixture]
    public class GivenUpsertsItShouldBeVerified
    {
        [SetUp]
        public void GivenUpsertCommands()
        {
            _verificationWriter = new VerificationUpsertWriter();

            _verificationWriter.Call(Procedure());
            _verificationWriter.Upsert(Identity(), TableValues());
        }

        private VerificationUpsertWriter _verificationWriter;

        private static CallProcedure Procedure()
        {
            return CallProcedure.Create("Exist",
                                        new ProcedureParameter("@Param1", DbType.String, "Hello"));
        }

        private static UpsertCommand TableValues()
        {
            return UpsertCommand.On(UpsertVector.WithValue("Col1", 5),
                                    UpsertVector.WithValue("Col2", 6));
        }

        private static KeyIdentity Identity()
        {
            return KeyIdentity.On("Table1", UpsertVector.WithValue("Key", 14));
        }

        [TestCase("Exist")]
        [TestCase("WillNotExist", ExpectedException = typeof(VerificationFailedException))]
        public void WhenTestingForProcedure(string procedureName)
        {
            var callProcedure =
                CallProcedure.Create(procedureName,
                                     new ProcedureParameter("@Param1", DbType.String, "Hello"));
            _verificationWriter.StartVerification()
                               .MatchUpsert(Identity(), TableValues())
                               .MatchProcedure(callProcedure)
                               .Verify();
        }

        [Test]
        [ExpectedException(typeof(VerificationFailedException))]
        public void WhenTestingForProcedureWithoutParameterItShouldThrowException()
        {
            var callProcedure =
                CallProcedure.Create("Exist",
                                     new ProcedureParameter("@ParamWillNotExist", DbType.String, "Hello"));
            _verificationWriter.StartVerification()
                               .MatchUpsert(Identity(), TableValues())
                               .MatchProcedure(callProcedure)
                               .Verify();
        }

        [Test]
        [ExpectedException(typeof(VerificationFailedException))]
        public void WhenTestingForTableWithoutExactValuesItShouldThrowException()
        {
            var values = UpsertCommand.On(UpsertVector.WithValue("Col1", 5));
            _verificationWriter.StartVerification()
                               .MatchUpsert(Identity(), values)
                               .MatchProcedure(Procedure())
                               .Verify();
        }

        [Test]
        public void WhenTestingForTableMisalignedValuesItShoulVerify()
        {
            var values = UpsertCommand.On(UpsertVector.WithValue("Col2", 6),
                                          UpsertVector.WithValue("Col1", 5));
            _verificationWriter.StartVerification()
                               .MatchUpsert(Identity(), values)
                               .MatchProcedure(Procedure())
                               .Verify();
        }
    }
}
