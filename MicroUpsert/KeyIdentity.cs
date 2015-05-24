using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroUpsert
{
    public sealed class KeyIdentity
    {
        private static readonly StringComparer InvariantIgnoreCasingComparer = StringComparer.InvariantCultureIgnoreCase;
        private readonly UpsertVector[] _identities;

        private KeyIdentity(string tableName, UpsertVector[] identities)
        {
            TableName = tableName;
            _identities = (UpsertVector[]) identities.Clone();
        }

        public string TableName { get; private set; }

        public IEnumerable<UpsertVector> Identities
        {
            get { return _identities; }
        }

        public static KeyIdentity On(string tableName, params UpsertVector[] identities)
        {
            return new KeyIdentity(tableName, identities);
        }

        private bool Equals(KeyIdentity other)
        {
            return Equals(_identities, other._identities)
                   && InvariantIgnoreCasingComparer.Equals(TableName, other.TableName);
        }

        private static bool Equals(IEnumerable<UpsertVector> a, IEnumerable<UpsertVector> b)
        {
            return a.All(x =>
            {
                var y = b.FirstOrDefault(_ => _.Equals(x));
                return y != null && x.Value.Equals(y.Value);
            });
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var tableIdentity = obj as KeyIdentity;
            return tableIdentity != null && Equals(tableIdentity);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                Func<int, UpsertVector, int> identity = (s, _) => s ^ _.GetHashCode() ^ _.Value.GetHashCode();
                return (_identities.Aggregate(0, identity)*397) ^ InvariantIgnoreCasingComparer.GetHashCode(TableName);
            }
        }
    }
}
