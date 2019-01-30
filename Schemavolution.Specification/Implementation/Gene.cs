using Schemavolution.Evolve.Providers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;

namespace Schemavolution.Specification.Implementation
{
    public abstract class Gene
    {
        private readonly Lazy<BigInteger> _sha256Hash;
        private readonly ImmutableList<Gene> _prerequisites;

        protected ImmutableList<Gene> Prerequisites => _prerequisites;

        protected Gene(ImmutableList<Gene> prerequisites)
        {
            _sha256Hash = new Lazy<BigInteger>(ComputeSha256Hash);
            _prerequisites = prerequisites;
        }

        public abstract IEnumerable<Gene> AllPrerequisites { get; }
        public abstract string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph, IDatabaseProvider provider);
        public abstract string[] GenerateRollbackSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph, IDatabaseProvider provider);
        internal abstract GeneMemento GetMemento();
        protected abstract BigInteger ComputeSha256Hash();

        internal virtual void AddToParent()
        {
        }

        internal BigInteger Sha256Hash => _sha256Hash.Value;

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
                return Equals((Gene)obj);
            return base.Equals(obj);
        }

        public bool Equals(Gene other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other.Sha256Hash == this.Sha256Hash;
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(Sha256Hash.ToByteArray(), 0);
        }
    }
}
