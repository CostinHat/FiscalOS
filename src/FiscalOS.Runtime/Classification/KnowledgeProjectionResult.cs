using System.Collections.Generic;
using FiscalOS.Core.Classification;

namespace FiscalOS.Runtime.Classification;

public sealed record KnowledgeProjectionResult
{
    public IReadOnlyList<ExplanationNode> Nodes { get; }

    public IReadOnlyList<ExplanationEdge> Edges { get; }

    public ExplanationNarrative Narrative { get; }

    public KnowledgeProjectionResult(
        IReadOnlyList<ExplanationNode> nodes,
        IReadOnlyList<ExplanationEdge> edges,
        ExplanationNarrative narrative)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);
        ArgumentNullException.ThrowIfNull(narrative);

        var ids = new HashSet<string>();
        foreach (var node in nodes)
        {
            if (!ids.Add(node.Id))
            {
                throw new InvalidOperationException($"Duplicate node id '{node.Id}'.");
            }
        }

        foreach (var edge in edges)
        {
            if (!ids.Contains(edge.FromNodeId))
            {
                throw new InvalidOperationException(
                    $"Edge references unknown node id '{edge.FromNodeId}'.");
            }

            if (!ids.Contains(edge.ToNodeId))
            {
                throw new InvalidOperationException(
                    $"Edge references unknown node id '{edge.ToNodeId}'.");
            }
        }

        Nodes = nodes;
        Edges = edges;
        Narrative = narrative;
    }
}
