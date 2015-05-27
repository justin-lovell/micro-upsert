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
            var pipeline = new ListeningUpsertWriter();

            // act
            pipeline.Upsert(identity, vector);

            // assert
            Assert.That(pipeline.UpsertVectors, Has.Count.EqualTo(1));

            var upsert = pipeline.UpsertVectors[0];

            Assert.That(upsert.Item1, Is.EqualTo(identity));

            Assert.That(upsert.Item2.Values, Has.Count.EqualTo(1));
            Assert.That(upsert.Item2.Values[0], Is.EqualTo(columnValues));
        }
    }
}
