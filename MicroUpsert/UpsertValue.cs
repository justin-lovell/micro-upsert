namespace MicroUpsert
{
    public abstract class UpsertValue
    {
        public static UpsertValue Static(object value)
        {
            return new StaticUpsertValue(value);
        }
    }
}