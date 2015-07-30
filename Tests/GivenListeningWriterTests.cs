using System.Linq;
using NUnit.Framework;

namespace MicroUpsert
{
    [TestFixture]
    public class GivenListeningWriterTests
    {
        [Test]
        public void WhenUpsertItShouldEcho()
        {
            var identity = KeyIdentity.On("Test", UpsertVector.WithValue("Id", 12));
            var columnValues = UpsertVector.WithValue("Col1", 12);
            var vector = UpsertCommand.On(columnValues);

            // arrange
            var pipeline = new VerificationUpsertWriter();

            // act
            pipeline.Upsert(identity, vector);

            // assert
            pipeline.StartVerification()
                    .MatchUpsert(KeyIdentity.On("Test",
                                                UpsertVector.WithValue("Id", 12)),
                                 UpsertCommand.On(UpsertVector.WithValue("Col1", 12)))
                    .Verify();
        }
    }
}
