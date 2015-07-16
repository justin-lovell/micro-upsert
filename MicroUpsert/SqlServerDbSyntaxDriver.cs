using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MicroUpsert
{
    public class SqlServerDbSyntaxDriver : IDbSyntaxDriver
    {
        public void DoUpsert(DbCommandController controller, KeyIdentity identity, UpsertCommand command)
        {
            var identities =
                identity.Identities
                        .ToDictionary(_ => _.ColumnName,
                                      _ => controller.GenerateUpsertParameter((StaticUpsertValue) _.Value)
                    );
            var settings =
                command.Values
                       .ToDictionary(_ => _.ColumnName,
                                     _ => controller.GenerateUpsertParameter((StaticUpsertValue) _.Value)
                    );

            /*
                INSERT INTO <table>
                SELECT <natural keys>, <other stuff...>
                WHERE NOT EXISTS
                   -- no race condition here
                   ( SELECT 1 FROM <table> WHERE <natural keys> )

                IF @@ROWCOUNT = 0 BEGIN
                  UPDATE ...
                  WHERE <natural keys>
                END
             */

            InsertHeader(controller, identity, identities, settings);
            SelectInsertHeader(controller, identities, settings);
            ConcurrentInsertWhereLock(controller, identity, identities);

            controller.BufferWriter.WriteLine();
            controller.BufferWriter.WriteLine("IF @@ROWCOUNT = 0 BEGIN");
            controller.BufferWriter.WriteLine("\t UPDATE [{0}]", identity.TableName);
            UpdateSetters(controller, settings);
            InsertWhereClauseForIdentity(controller, identities);
            controller.BufferWriter.WriteLine();
            controller.BufferWriter.WriteLine("END");
        }

        private void UpdateSetters(DbCommandController controller, Dictionary<string, string> settings)
        {
            var separator = "\t SET";
            foreach (var setting in settings)
            {
                controller.BufferWriter.Write("{0} [{1}] = {2}", separator, setting.Key, setting.Value);
                separator = ",";
            }
            controller.BufferWriter.WriteLine();
        }

        public void DoProcedure(DbCommandController controller, CallProcedure details)
        {
            controller.BufferWriter.Write("exec {0}", details.ProcedureName);

            string seperator = "";
            foreach (var procedureParameter in details.Parameters)
            {
                var staticValue = UpsertValue.Static(procedureParameter.Value);
                var parameter = controller.GenerateUpsertParameter((StaticUpsertValue) staticValue);

                controller.BufferWriter.WriteLine(seperator);
                controller.BufferWriter.Write("\t{0}={1}", procedureParameter.ParameterName, parameter);
                seperator = ",";
            }

            controller.BufferWriter.WriteLine();
        }

        public string GenerateParameterName(int count)
        {
            return string.Format(CultureInfo.InvariantCulture, "@p{0}", count);
        }

        private void ConcurrentInsertWhereLock(
            DbCommandController controller,
            KeyIdentity keyIdentity,
            Dictionary<string, string> identities)
        {
            controller.BufferWriter.Write("\t WHERE NOT EXISTS (SELECT TOP 1 42 FROM [{0}] ", keyIdentity.TableName);

            InsertWhereClauseForIdentity(controller, identities);
            controller.BufferWriter.WriteLine(")");
        }

        private static void InsertWhereClauseForIdentity(DbCommandController controller, Dictionary<string, string> identities)
        {
            var separator = "WHERE";
            foreach (var identity in identities)
            {
                controller.BufferWriter.Write("{0} [{1}] = {2}", separator, identity.Key, identity.Value);
                separator = " AND";
            }
        }

        private static void SelectInsertHeader(
            DbCommandController controller,
            Dictionary<string, string> identities,
            Dictionary<string, string> settings)
        {
            var separator = "\t SELECT ";
            foreach (var value in identities.Values.Concat(settings.Values))
            {
                controller.BufferWriter.Write(separator);
                controller.BufferWriter.Write(value);
                separator = ", ";
            }
            controller.BufferWriter.WriteLine();
        }

        private static void InsertHeader(
            DbCommandController controller,
            KeyIdentity identity,
            Dictionary<string, string> identities,
            Dictionary<string, string> settings)
        {
            controller.BufferWriter.Write("INSERT INTO [{0}] ", identity.TableName);

            var separator = "(";
            foreach (var key in identities.Keys.Concat(settings.Keys))
            {
                controller.BufferWriter.Write("{0}[{1}]", separator, key);
                separator = ", ";
            }
            controller.BufferWriter.WriteLine(")");
        }
    }
}