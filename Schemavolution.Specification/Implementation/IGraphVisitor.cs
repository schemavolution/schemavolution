using System;
using System.Collections.Immutable;

namespace Schemavolution.Specification.Implementation
{
    public interface IGraphVisitor
    {
        ImmutableList<Gene> PullPrerequisitesForward(Gene gene, Gene origin, Func<Gene, bool> canOptimize);
    }
}
