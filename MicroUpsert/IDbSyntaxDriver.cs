namespace MicroUpsert
{
    public interface IDbSyntaxDriver
    {
        void DoUpsert(DbSyntaxOutput dbSyntaxOutput, KeyIdentity identity, UpsertCommand command);
        void DoProcedure(DbSyntaxOutput dbSyntaxOutput, CallProcedure details);
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
