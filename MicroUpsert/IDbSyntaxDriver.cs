using System.Globalization;

namespace MicroUpsert
{
    public interface IDbSyntaxDriver
    {
        void DoUpsert(DbCommandController dbCommandController, KeyIdentity identity, UpsertCommand command);
        void DoProcedure(DbCommandController dbCommandController, CallProcedure details);
        string GenerateParameterName(int count);
    }

    public class SqlServerDbSyntaxDriver : IDbSyntaxDriver
    {
        public void DoUpsert(DbCommandController dbCommandController, KeyIdentity identity, UpsertCommand command)
        {
            throw new System.NotImplementedException();
        }

        public void DoProcedure(DbCommandController dbCommandController, CallProcedure details)
        {
            throw new System.NotImplementedException();
        }

        public string GenerateParameterName(int count)
        {
            return string.Format(CultureInfo.InvariantCulture, "@p{0}", count);
        }
    }


    /*
     * INSERT INTO <table>
SELECT <natural keys>, <other stuff...>
WHERE NOT EXISTS
   -- no race condition here
   ( SELECT 1 FROM <table> WHERE <natural keys> )

IF @@ROWCOUNT = 0 BEGIN
  UPDATE ...
  WHERE <natural keys>
END*/
}
