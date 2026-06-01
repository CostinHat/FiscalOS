using System.Collections.Generic;
using System.Linq;

namespace FiscalOS.LegalKnowledge;

public sealed record AuditGraph
{
    private readonly Dictionary<string, AuditNode> _nodesById;

    public IReadOnlyList<AuditNode> Nodes { get; }

    public IReadOnlyList<AuditEdge> Edges { get; }

    public AuditGraph(
        IReadOnlyList<AuditNode> nodes,
        IReadOnlyList<AuditEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);

        _nodesById = new Dictionary<string, AuditNode>();

        foreach (var node in nodes)
        {
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

    public AuditNode? NodeById(string id) =>
        _nodesById.TryGetValue(id, out var node) ? node : null;

    public IReadOnlyList<AuditNode> NodesOfType(AuditNodeType type) =>
        Nodes.Where(node => node.Type == type).ToList();

    public IReadOnlyList<AuditEdge> EdgesFrom(string nodeId) =>
        Edges.Where(edge => edge.FromNodeId == nodeId).ToList();

    public IReadOnlyList<AuditEdge> EdgesTo(string nodeId) =>
        Edges.Where(edge => edge.ToNodeId == nodeId).ToList();
}
