using System;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Audit_Graph
{
    private static AuditNode Node(string id, AuditNodeType type) =>
        new(id, type, $"{type} {id}");

    [Fact]
    public void Builds_and_traverses_a_provenance_chain()
    {
        var nodes = new[]
        {
            Node("fact", AuditNodeType.Fact),
            Node("rule", AuditNodeType.Rule),
            Node("evidence", AuditNodeType.Evidence),
            Node("conflict", AuditNodeType.ConflictResolution),
            Node("purpose", AuditNodeType.Purpose),
            Node("decision", AuditNodeType.Decision)
        };
        var edges = new[]
        {
            new AuditEdge("rule", "evidence", AuditEdgeType.Produces),
            new AuditEdge("evidence", "fact", AuditEdgeType.Uses),
            new AuditEdge("evidence", "purpose", AuditEdgeType.Cites),
            new AuditEdge("conflict", "evidence", AuditEdgeType.Eliminates),
            new AuditEdge("purpose", "decision", AuditEdgeType.Justifies),
            new AuditEdge("decision", "evidence", AuditEdgeType.DerivedFrom)
        };

        var graph = new AuditGraph(nodes, edges);

        Assert.Equal(6, graph.Nodes.Count);
        Assert.Equal(6, graph.Edges.Count);

        var fromRule = Assert.Single(graph.EdgesFrom("rule"));
        Assert.Equal("evidence", fromRule.ToNodeId);
        Assert.Equal(AuditEdgeType.Produces, fromRule.Type);
    }

    [Fact]
    public void Queries_return_expected_sets()
    {
        var graph = new AuditGraph(
            new[]
            {
                Node("fact", AuditNodeType.Fact),
                Node("evidence", AuditNodeType.Evidence),
                Node("decision", AuditNodeType.Decision)
            },
            new[]
            {
                new AuditEdge("evidence", "fact", AuditEdgeType.Uses),
                new AuditEdge("decision", "evidence", AuditEdgeType.DerivedFrom)
            });

        Assert.Equal(AuditNodeType.Evidence, graph.NodeById("evidence")!.Type);
        Assert.Null(graph.NodeById("missing"));
        Assert.Single(graph.NodesOfType(AuditNodeType.Fact));

        var toEvidence = Assert.Single(graph.EdgesTo("evidence"));
        Assert.Equal("decision", toEvidence.FromNodeId);
    }

    [Fact]
    public void Empty_graph_is_valid()
    {
        var graph = new AuditGraph(Array.Empty<AuditNode>(), Array.Empty<AuditEdge>());

        Assert.Empty(graph.Nodes);
        Assert.Empty(graph.Edges);
        Assert.Null(graph.NodeById("anything"));
        Assert.Empty(graph.NodesOfType(AuditNodeType.Decision));
    }

    [Fact]
    public void Nodes_and_edges_compare_by_value()
    {
        var a = Node("fact", AuditNodeType.Fact);
        var b = Node("fact", AuditNodeType.Fact);
        Assert.Equal(a, b);
        Assert.NotEqual(a, a with { Id = "other" });

        var e1 = new AuditEdge("x", "y", AuditEdgeType.Produces);
        var e2 = new AuditEdge("x", "y", AuditEdgeType.Produces);
        Assert.Equal(e1, e2);
        Assert.NotEqual(e1, e1 with { Type = AuditEdgeType.Cites });
    }

    [Fact]
    public void Duplicate_node_id_fails_fast()
    {
        var nodes = new[]
        {
            Node("dup", AuditNodeType.Fact),
            Node("dup", AuditNodeType.Decision)
        };

        Assert.Throws<InvalidOperationException>(
            () => new AuditGraph(nodes, Array.Empty<AuditEdge>()));
    }

    [Fact]
    public void Edge_referencing_unknown_node_fails_fast()
    {
        var nodes = new[] { Node("fact", AuditNodeType.Fact), Node("evidence", AuditNodeType.Evidence) };
        var edges = new[] { new AuditEdge("evidence", "ghost", AuditEdgeType.Uses) };

        Assert.Throws<InvalidOperationException>(
            () => new AuditGraph(nodes, edges));
    }
}
