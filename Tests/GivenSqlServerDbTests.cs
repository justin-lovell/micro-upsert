using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;

namespace MicroUpsert
{
    [TestFixture]
    public class GivenSqlServerDbTests
    {
        private Func<IDbConnection> _createConnectionFactory; 
        private IDbSyntaxDriver _dbSyntax;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            var dbProvider = DbProviderFactories.GetFactory("System.Data.SqlClient");
            const string connectionString = "Data Source=.;Integrated Security=True;Initial Catalog=MicroUpsertTestDb";

            _dbSyntax = new SqlServerDbSyntaxDriver();
            _createConnectionFactory = () =>
            {
                var connection = dbProvider.CreateConnection();
                connection.ConnectionString = connectionString;
                return connection;
            };
        }

        [Test]
        public void WhenUpsertItShouldSucceed()
        {
            var writer = new DbUpsertWriter(_dbSyntax, _createConnectionFactory);

            writer.Upsert(KeyIdentity.On("Table1", UpsertVector.WithValue("TheGuid", Guid.NewGuid())),
                          UpsertCommand.On(UpsertVector.WithValue("ColA", Guid.NewGuid())));

            writer.Go();
        }

        [Test]
        public void WhenCallingGoOnEmptyCommandItShouldNotConnectToServer()
        {
            var writer = new DbUpsertWriter(_dbSyntax, _createConnectionFactory);

            writer.Go();
        }

        [Test]
        public void WhenCallProcedureItShouldEchoResultBack()
        {
            string randomString = Guid.NewGuid().ToString();

            var writer = new DbUpsertWriter(_dbSyntax, _createConnectionFactory);
            writer.Call(CallProcedure.Create("Echo", new ProcedureParameter("@Msg", DbType.String, randomString)));

            bool wasResults = false;
            string col1Result = null;
            writer.GoAndRead(reader =>
            {
                wasResults = reader.Read();
                if (wasResults)
                    col1Result = reader[0].ToString();
            });

            Assert.That(wasResults, Is.True);
            Assert.That(col1Result, Is.EqualTo(randomString));
        }
    }
}