using System;
using System.Data.Common;
using NUnit.Framework;

namespace MicroUpsert
{
    [TestFixture]
    public class GivenSqlServerDbTests
    {
        private DbProviderFactory _dbProvider;
        private IDbSyntaxDriver _dbSyntax;
        private string _connectionString;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _dbProvider = DbProviderFactories.GetFactory("System.Data.SqlClient");
            _dbSyntax = new SqlServerDbSyntaxDriver();
            _connectionString = "Data Source=.;Integrated Security=True;Initial Catalog=MicroUpsertTestDb";
        }

        [Test]
        public void WhenUpsertItShouldSucceed()
        {
            var writer = new DbUpsertWriter(_dbProvider, _dbSyntax, _connectionString);

            writer.Upsert(KeyIdentity.On("Table1", UpsertVector.WithValue("TheGuid", Guid.NewGuid())),
                          UpsertCommand.On(UpsertVector.WithValue("ColA", Guid.NewGuid())));

            writer.Go();
        }
    }
}