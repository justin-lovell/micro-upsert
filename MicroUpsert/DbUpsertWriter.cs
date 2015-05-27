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
            DbProviderFactory providerFactory,
            IDbSyntaxDriver dbSyntaxDriver,
            string connectionString)
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

        private DbCommandController CreateDbSyntaxOutput()
        {
            Func<StaticUpsertValue, string> injectParameter =
                (value) =>
                {
                    DbParameter parameter;
                    if (_valueToParameterDictionary.TryGetValue(value, out parameter))
                    {
                        return parameter.ParameterName;
                    }

                    var dbParam = _providerFactory.CreateParameter();

                    dbParam.ParameterName =
                        _dbSyntaxDriver.GenerateParameterName(_valueToParameterDictionary.Count);
                    dbParam.Value = value.Value;

                    _valueToParameterDictionary.Add(value, dbParam);

                    return dbParam.ParameterName;
                };
            var dbSyntaxOutput = new DbCommandController(_bufferWriter, injectParameter);
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

                    foreach (var parameter in _valueToParameterDictionary.Values)
                    {
                        command.Parameters.Add(parameter);
                    }

                    connection.Open();
                    commandCallback(command);
                }
            }

            _bufferWriter = new StringWriter();
            _valueToParameterDictionary = new Dictionary<StaticUpsertValue, DbParameter>();
        }
    }
}
