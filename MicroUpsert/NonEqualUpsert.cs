namespace MicroUpsert
{
    public sealed class NonEqualUpsert
    {
        public KeyIdentity KeyIdentity { get; private set; }
        public UpsertCommand Actual { get; private set; }
        public UpsertCommand Expected { get; private set; }

        public NonEqualUpsert(KeyIdentity keyIdentity, UpsertCommand actual, UpsertCommand expected)
        {
            KeyIdentity = keyIdentity;
            Actual = actual;
            Expected = expected;
        }

        public override string ToString()
        {
            return string.Format("{0} :: Expected: [{1}] but got [{2}]", KeyIdentity, Expected, Actual);
        }
    }
}