using System.Collections.Generic;
using System.Linq;
using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public sealed record PurposeAwareExplanation(
    DecisionExplanation Explanation,
    PurposeGraph PurposeGraph)
{
    public bool HasPurposeContext =>
        PurposeGraph.NodesOfKind(PurposeNodeType.Norm).Count > 0;

    public IReadOnlyList<PurposeReference> GoverningPurposes()
    {
        var normNodes = PurposeGraph.NodesOfKind(PurposeNodeType.Norm);
        var references = new List<PurposeReference>();

        foreach (var citation in Explanation.LegalBasis.GoverningCitations)
        {
            foreach (var norm in normNodes.Where(norm => Equals(norm.Citation, citation)))
            {
                references.Add(new PurposeReference(citation, norm));
            }
        }

        return references;
    }
}
