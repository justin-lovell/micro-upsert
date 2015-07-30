namespace MicroUpsert
{
    public interface IVerificationBuilder
    {
        IVerificationBuilder MatchUpsert(KeyIdentity identity, UpsertCommand command);
        IVerificationBuilder MatchProcedure(CallProcedure details);
        void Verify();
    }
}