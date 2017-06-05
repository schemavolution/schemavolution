using Schemavolution.Specification.Implementation;
using System.Collections.Generic;
using System.Linq;

namespace Schemavolution.Specification
{
    public class ForeignKeySpecification : Specification
    {
        internal override IEnumerable<Gene> Genes => Enumerable.Empty<Gene>();

        internal ForeignKeySpecification(EvolutionHistoryBuilder geneHistoryBuilder) :
            base(geneHistoryBuilder)
        {
        }
    }
}