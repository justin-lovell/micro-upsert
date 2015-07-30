namespace MicroUpsert
{
    public sealed class NonEqualProcedure
    {
        public string ProcedureName { get; private set; }
        public CallProcedure Actual { get; private set; }
        public CallProcedure Expected { get; private set; }

        public NonEqualProcedure(string procedureName, CallProcedure actual, CallProcedure expected)
        {
            ProcedureName = procedureName;
            Actual = actual;
            Expected = expected;
        }

        public override string ToString()
        {
            return string.Format("{0} :: Expected: [{1}] but got [{2}]", ProcedureName, Expected, Actual);
        }
    }
}