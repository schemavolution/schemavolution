using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;

namespace Schemavolution.Specification.Implementation
{
    public class GeneMemento
    {
        public string Type { get; }
        public ImmutableDictionary<string, string> Attributes { get; }
        public BigInteger HashCode { get; }
        public PrerequisiteDictionary Prerequisites { get; }

        public GeneMemento(string type, IDictionary<string, string> attributes, BigInteger hashCode, IDictionary<string, IEnumerable<BigInteger>> prerequisites)
        {
            Type = type;
            Attributes = attributes.ToImmutableDictionary();
            HashCode = hashCode;
            Prerequisites = new PrerequisiteDictionary(prerequisites);
        }
    }
}