using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;

namespace MicroUpsert
{
    public sealed class DbUpsertWriter : UpsertWriter
    {
        private readonly IDbSyntaxDriver _dbSyntaxDriver;
        private readonly Func<IDbConnection> _dbConnectionFactory;
        private StringWriter _bufferWriter = new StringWriter();

        private Dictionary<StaticUpsertValue, Tuple<string, object>> _valueToParameterDictionary =
            new Dictionary<StaticUpsertValue, Tuple<string, object>>();

        public DbUpsertWriter(
            IDbSyntaxDriver dbSyntaxDriver,
            Func<IDbConnection> dbConnectionFactory)
        {
            _dbSyntaxDriver = dbSyntaxDriver;
            _dbConnectionFactory = dbConnectionFactory;
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
            Func<StaticUpsertValue, string> generateUpsertParameter =
                value =>
                {
                    Tuple<string, object> parameter;
                    if (_valueToParameterDictionary.TryGetValue(value, out parameter))
                    {
                        return parameter.Item1;
                    }

                    string parameterName = _dbSyntaxDriver.GenerateParameterName(_valueToParameterDictionary.Count);
                    object parameterValue = value.Value;

                    var tuple = new Tuple<string, object>(parameterName, parameterValue);

                    _valueToParameterDictionary.Add(value, tuple);

                    return tuple.Item1;
                };
            return new DbCommandController(_bufferWriter, generateUpsertParameter);
        }

        private void ExecuteDbCommand(Action<IDbCommand> commandCallback)
        {
            using (var connection = _dbConnectionFactory())
            {
                using (var command = connection.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = _bufferWriter.ToString();

                    foreach (var parameter in _valueToParameterDictionary.Values)
                    {
                        var dbDataParameter = command.CreateParameter();

                        dbDataParameter.ParameterName = parameter.Item1;
                        dbDataParameter.Value = parameter.Item2;

                        command.Parameters.Add(dbDataParameter);
                    }

                    if (!string.IsNullOrEmpty(command.CommandText))
                    {
                        connection.Open();
                        commandCallback(command);
                    }
                }
            }

            _bufferWriter = new StringWriter();
            _valueToParameterDictionary = new Dictionary<StaticUpsertValue, Tuple<string, object>>();
        }
    }
}
