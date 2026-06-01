using System.Collections.Generic;
using System.Linq;

namespace FiscalOS.LegalKnowledge;

public sealed record PurposeGraph
{
    private readonly Dictionary<string, PurposeNode> _nodesById;

    public IReadOnlyList<PurposeNode> Nodes { get; }

    public IReadOnlyList<PurposeEdge> Edges { get; }

    public PurposeGraph(
        IReadOnlyList<PurposeNode> nodes,
        IReadOnlyList<PurposeEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);

        _nodesById = new Dictionary<string, PurposeNode>();

        foreach (var node in nodes)
        {
            if (node.Kind == PurposeNodeType.Norm && node.Citation is null)
            {
                throw new InvalidOperationException(
                    $"Norm node '{node.Id}' must have a citation.");
            }

            if (node.Kind != PurposeNodeType.Norm && node.Citation is not null)
            {
                throw new InvalidOperationException(
                    $"Non-norm node '{node.Id}' must not have a citation.");
            }

            if (!_nodesById.TryAdd(node.Id, node))
            {
                throw new InvalidOperationException(
                    $"Duplicate node id '{node.Id}'.");
            }
        }

        foreach (var edge in edges)
        {
            if (!_nodesById.ContainsKey(edge.FromNodeId))
            {
                throw new InvalidOperationException(
                    $"Edge references unknown node id '{edge.FromNodeId}'.");
            }

            if (!_nodesById.ContainsKey(edge.ToNodeId))
            {
                throw new InvalidOperationException(
                    $"Edge references unknown node id '{edge.ToNodeId}'.");
            }
        }

        Nodes = nodes;
        Edges = edges;
    }

    public PurposeNode? NodeById(string id) =>
        _nodesById.TryGetValue(id, out var node) ? node : null;

    public IReadOnlyList<PurposeNode> NodesOfKind(PurposeNodeType kind) =>
        Nodes.Where(node => node.Kind == kind).ToList();

    public IReadOnlyList<PurposeEdge> EdgesFrom(string nodeId) =>
        Edges.Where(edge => edge.FromNodeId == nodeId).ToList();

    public IReadOnlyList<PurposeEdge> EdgesTo(string nodeId) =>
        Edges.Where(edge => edge.ToNodeId == nodeId).ToList();
}
