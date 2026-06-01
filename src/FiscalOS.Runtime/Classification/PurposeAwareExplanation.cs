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

    public IReadOnlyList<PurposeChain> GoverningPurposeChains()
    {
        var chains = new List<PurposeChain>();

        foreach (var reference in GoverningPurposes())
        {
            ExtendChain(
                reference.GoverningCitation,
                new List<PurposeNode> { reference.PurposeNode },
                new HashSet<string> { reference.PurposeNode.Id },
                chains);
        }

        return chains;
    }

    private void ExtendChain(
        LegalCitation governingCitation,
        List<PurposeNode> path,
        HashSet<string> visited,
        List<PurposeChain> chains)
    {
        var current = path[^1];

        var nextNodes = PurposeGraph
            .EdgesFrom(current.Id)
            .Where(edge => edge.Relation == PurposeRelationType.Serves)
            .Select(edge => PurposeGraph.NodeById(edge.ToNodeId))
            .Where(node => node is not null && !visited.Contains(node.Id))
            .Select(node => node!)
            .ToList();

        if (nextNodes.Count == 0)
        {
            chains.Add(new PurposeChain(governingCitation, path.ToList()));
            return;
        }

        foreach (var next in nextNodes)
        {
            path.Add(next);
            visited.Add(next.Id);

            ExtendChain(governingCitation, path, visited, chains);

            path.RemoveAt(path.Count - 1);
            visited.Remove(next.Id);
        }
    }
}
