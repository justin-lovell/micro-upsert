namespace MicroUpsert
{
    public interface IDbSyntaxDriver
    {
        void DoUpsert(DbCommandController controller, KeyIdentity identity, UpsertCommand command);
        void DoProcedure(DbCommandController controller, CallProcedure details);
        string GenerateParameterName(int count);
    }
}
