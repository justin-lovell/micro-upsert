using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;

namespace MicroUpsert
{
    public sealed class DbUpsertWriter : UpsertWriter
    {
        private readonly string _connectionString;
        private readonly IDbSyntaxDriver _dbSyntaxDriver;
        private readonly DbProviderFactory _providerFactory;
        private StringWriter _bufferWriter = new StringWriter();

        private Dictionary<StaticUpsertValue, DbParameter> _valueToParameterDictionary =
            new Dictionary<StaticUpsertValue, DbParameter>();

        public DbUpsertWriter(
            string connectionString,
            DbProviderFactory providerFactory,
            IDbSyntaxDriver dbSyntaxDriver)
        {
            _connectionString = connectionString;
            _providerFactory = providerFactory;
            _dbSyntaxDriver = dbSyntaxDriver;
        }

        public override void Upsert(KeyIdentity identity, UpsertCommand command)
        {
            var dbSyntaxOutput = CreateDbSyntaxOutput();
            _dbSyntaxDriver.DoUpsert(dbSyntaxOutput, identity, command);
        }

        public override void Call(CallProcedure details)
        {
            var dbSyntaxOutput = CreateDbSyntaxOutput();
            _dbSyntaxDriver.DoProcedure(dbSyntaxOutput, details);
        }

        public override void Go()
        {
            ExecuteDbCommand(command => command.ExecuteNonQuery());
        }

        public override void GoAndRead(Action<IDataReader> readCallback)
        {
            ExecuteDbCommand(command =>
            {
                var dbDataReader = command.ExecuteReader();
                readCallback(dbDataReader);
            });
        }

        private DbSyntaxOutput CreateDbSyntaxOutput()
        {
            Func<string> generateUniqueParameterName = () => "p" + _valueToParameterDictionary.Count;
            Action<string, StaticUpsertValue> insertParameter =
                (parameterName, value) =>
                {
                    if (_valueToParameterDictionary.ContainsKey(value))
                    {
                        return;
                    }

                    var dbParam = _providerFactory.CreateParameter();

                    dbParam.ParameterName = parameterName;
                    dbParam.Value = value.Value;

                    _valueToParameterDictionary.Add(value, dbParam);
                };

            var dbSyntaxOutput = new DbSyntaxOutput(_bufferWriter, generateUniqueParameterName, insertParameter);
            return dbSyntaxOutput;
        }

        private void ExecuteDbCommand(Action<DbCommand> commandCallback)
        {
            using (var connection = _providerFactory.CreateConnection())
            {
                using (var command = _providerFactory.CreateCommand())
                {
                    connection.ConnectionString = _connectionString;

                    command.Connection = connection;
                    command.CommandText = _bufferWriter.ToString();

                    connection.Open();
                    commandCallback(command);
                }
            }

            _bufferWriter = new StringWriter();
            _valueToParameterDictionary = new Dictionary<StaticUpsertValue, DbParameter>();
        }
    }
}
