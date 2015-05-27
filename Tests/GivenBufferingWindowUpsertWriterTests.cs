using System.Linq;
using NUnit.Framework;

namespace MicroUpsert
{
    [TestFixture]
    public class GivenBufferingWindowUpsertWriterTests
    {
        [Test]
        public void WhenMultipleCommandsOnTheSameIdentityOnSameGoItShouldMergedIntoOneCommand()
        {
            var keyIdentity = KeyIdentity.On("Test", UpsertVector.WithValue("Id", 12));

            var column1 = UpsertVector.WithValue("Hello", "World");
            var column2 = UpsertVector.WithValue("Another", "World");

            // arrange
            var nextPipeline = new ListeningUpsertWriter();
            var bufferingPipeline = new BufferingWindowUpsertWriter(nextPipeline);

            // act

            bufferingPipeline.Upsert(keyIdentity, UpsertCommand.On(column1));
            bufferingPipeline.Upsert(keyIdentity, UpsertCommand.On(column2));
            bufferingPipeline.Go();

            // assert
            Assert.That(nextPipeline.UpsertVectors, Has.Count.EqualTo(1));

            var upsert = nextPipeline.UpsertVectors[0];

            Assert.That(upsert.Item1, Is.EqualTo(keyIdentity));

            Assert.That(upsert.Item2.Values, Has.Count.EqualTo(2));
            Assert.That(upsert.Item2.Values, Has.Member(column1));
            Assert.That(upsert.Item2.Values, Has.Member(column2));
        }

        [Test]
        public void WhenMultipleCommandsOnTheSameIdentityOnDifferentGoItShouldNotHaveMergedIntoOneCommand()
        {
            var keyIdentity = KeyIdentity.On("Test", UpsertVector.WithValue("Id", 12));

            var column1 = UpsertVector.WithValue("Hello", "World");
            var column2 = UpsertVector.WithValue("Another", "World");

            // arrange
            var nextPipeline = new ListeningUpsertWriter();
            var bufferingPipeline = new BufferingWindowUpsertWriter(nextPipeline);

            // act

            bufferingPipeline.Upsert(keyIdentity, UpsertCommand.On(column1));
            bufferingPipeline.Go();
            bufferingPipeline.Upsert(keyIdentity, UpsertCommand.On(column2));
            bufferingPipeline.Go();

            // assert
            Assert.That(nextPipeline.UpsertVectors, Has.Count.EqualTo(2));

            var upsert = nextPipeline.UpsertVectors[0];

            Assert.That(upsert.Item1, Is.EqualTo(keyIdentity));
            Assert.That(upsert.Item2.Values, Has.Count.EqualTo(1));
            Assert.That(upsert.Item2.Values, Has.Member(column1));


            upsert = nextPipeline.UpsertVectors[1];

            Assert.That(upsert.Item1, Is.EqualTo(keyIdentity));
            Assert.That(upsert.Item2.Values, Has.Count.EqualTo(1));
            Assert.That(upsert.Item2.Values, Has.Member(column2));
        }

        [Test]
        public void WhenMultipleCommandsButDifferentIdentitiesItShouldNotHaveMergedIntoOneCommand()
        {
            var keyIdentity1 = KeyIdentity.On("Test", UpsertVector.WithValue("Id", 12));
            var keyIdentity2 = KeyIdentity.On("Test", UpsertVector.WithValue("Id", 1));

            var column1 = UpsertVector.WithValue("Hello", "World");
            var column2 = UpsertVector.WithValue("Another", "World");

            // arrange
            var nextPipeline = new ListeningUpsertWriter();
            var bufferingPipeline = new BufferingWindowUpsertWriter(nextPipeline);

            // act
            bufferingPipeline.Upsert(keyIdentity1, UpsertCommand.On(column1));
            bufferingPipeline.Upsert(keyIdentity2, UpsertCommand.On(column2));
            bufferingPipeline.Go();

            // assert
            Assert.That(nextPipeline.UpsertVectors, Has.Count.EqualTo(2));

            var upsert = nextPipeline.UpsertVectors[0];

            Assert.That(upsert.Item1, Is.EqualTo(keyIdentity1));
            Assert.That(upsert.Item2.Values, Has.Count.EqualTo(1));
            Assert.That(upsert.Item2.Values, Has.Member(column1));


            upsert = nextPipeline.UpsertVectors[1];

            Assert.That(upsert.Item1, Is.EqualTo(keyIdentity2));
            Assert.That(upsert.Item2.Values, Has.Count.EqualTo(1));
            Assert.That(upsert.Item2.Values, Has.Member(column2));
        }

        [Test]
        public void WhenMultipleCommandsAreBetweenCallProcedureItShouldNotHaveMergedIntoOneCommand()
        {
            var keyIdentity = KeyIdentity.On("Test", UpsertVector.WithValue("Id", 12));

            var column1 = UpsertVector.WithValue("Hello", "World");
            var column2 = UpsertVector.WithValue("Another", "World");

            // arrange
            var nextPipeline = new ListeningUpsertWriter();
            var bufferingPipeline = new BufferingWindowUpsertWriter(nextPipeline);

            // act

            bufferingPipeline.Upsert(keyIdentity, UpsertCommand.On(column1));
            bufferingPipeline.Call(CallProcedure.Create("SayHello"));
            bufferingPipeline.Upsert(keyIdentity, UpsertCommand.On(column2));
            bufferingPipeline.Go();

            // assert
            Assert.That(nextPipeline.Procedures, Has.Count.EqualTo(1));
            Assert.That(nextPipeline.Procedures.First().ProcedureName, Is.EqualTo("SayHello"));

            Assert.That(nextPipeline.UpsertVectors, Has.Count.EqualTo(2));

            var upsert = nextPipeline.UpsertVectors[0];

            Assert.That(upsert.Item1, Is.EqualTo(keyIdentity));
            Assert.That(upsert.Item2.Values, Has.Count.EqualTo(1));
            Assert.That(upsert.Item2.Values, Has.Member(column1));


            upsert = nextPipeline.UpsertVectors[1];

            Assert.That(upsert.Item1, Is.EqualTo(keyIdentity));
            Assert.That(upsert.Item2.Values, Has.Count.EqualTo(1));
            Assert.That(upsert.Item2.Values, Has.Member(column2));
        }

        [Test]
        public void WhenTheGoIsCalledItShouldHavePopulatedTheNextPipeline()
        {
            // arrange
            var nextPipeline = new ListeningUpsertWriter();
            var bufferingPipeline = new BufferingWindowUpsertWriter(nextPipeline);

            // act
            bufferingPipeline.Upsert(KeyIdentity.On("Test", UpsertVector.WithValue("Id", 12)),
                                     UpsertCommand.On(UpsertVector.WithValue("Hello", "World")));
            bufferingPipeline.Go();

            // assert
            Assert.That(nextPipeline.UpsertVectors, Has.Count.EqualTo(1));
        }

        [Test]
        public void WhenTheGoIsNotCalledItShouldNotHavePopulatedTheNextPipeline()
        {
            // arrange
            var nextPipeline = new ListeningUpsertWriter();
            var bufferingPipeline = new BufferingWindowUpsertWriter(nextPipeline);

            // act
            bufferingPipeline.Upsert(KeyIdentity.On("Test", UpsertVector.WithValue("Id", 12)),
                                     UpsertCommand.On(UpsertVector.WithValue("Hello", "World")));

            // assert
            Assert.That(nextPipeline.UpsertVectors, Is.Empty);
        }
    }
}
