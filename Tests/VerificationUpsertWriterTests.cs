using System;
using NUnit.Framework;

namespace MicroUpsert
{
    [TestFixture]
    public class VerificationUpsertWriterTests
    {
        [Test]
        public void GivenUpsertWithMultipleValueItShouldVerifyIfSameUpsertCommandIsGiven()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            Guid guid = Guid.NewGuid();

            var keyIdentity = KeyIdentity.On("AuthToken", UpsertVector.WithValue("TokenId", guid));

            Func<UpsertCommand> generateCommand =
                () => UpsertCommand.On(UpsertVector.WithValue("ValidFromDateTime", dateTimeOffset),
                                       UpsertVector.WithValue("ValidFromDateTime", dateTimeOffset.AddDays(30)),
                                       UpsertVector.WithValue("Secret", guid),
                                       UpsertVector.WithValue("Roles", 4),
                                       UpsertVector.WithValue("ReferenceId", 5));


            var upsertWriter = new VerificationUpsertWriter();

            upsertWriter.Upsert(keyIdentity, generateCommand());
            upsertWriter.Go();

            upsertWriter.StartVerification()
                        .MatchUpsert(keyIdentity, generateCommand())
                        .Verify();
        }
    }
}