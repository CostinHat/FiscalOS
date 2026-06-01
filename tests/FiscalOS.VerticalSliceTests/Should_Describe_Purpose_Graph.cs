using System;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Purpose_Graph
{
    private static LegalCitation Citation(string article) =>
        new(LegalSourceType.Law, "Legea 227/2015", article, SpecificityLevel.Specific, new DateOnly(2024, 1, 1));

    private static PurposeNode Norm(string id, string article) =>
        new(id, PurposeNodeType.Norm, $"Norm {id}", Citation(article));

    private static PurposeNode Abstract(string id, PurposeNodeType kind) =>
        new(id, kind, $"{kind} {id}", null);

    [Fact]
    public void Builds_and_traverses_a_teleological_chain()
    {
        var nodes = new[]
        {
            Norm("norm", "Art. 47"),
            Abstract("obj", PurposeNodeType.Objective),
            Abstract("prin", PurposeNodeType.Principle),
            Abstract("val", PurposeNodeType.ProtectedValue)
        };
        var edges = new[]
        {
            new PurposeEdge("norm", "obj", PurposeRelationType.Serves),
            new PurposeEdge("obj", "prin", PurposeRelationType.Serves),
            new PurposeEdge("prin", "val", PurposeRelationType.Serves)
        };

        var graph = new PurposeGraph(nodes, edges);

        Assert.Equal(4, graph.Nodes.Count);
        Assert.Equal(3, graph.Edges.Count);

        var fromNorm = Assert.Single(graph.EdgesFrom("norm"));
        Assert.Equal("obj", fromNorm.ToNodeId);
        Assert.Equal(PurposeRelationType.Serves, fromNorm.Relation);
    }

    [Fact]
    public void Queries_return_expected_sets()
    {
        var graph = new PurposeGraph(
            new[]
            {
                Norm("norm", "Art. 47"),
                Abstract("obj", PurposeNodeType.Objective),
                Abstract("prin", PurposeNodeType.Principle)
            },
            new[]
            {
                new PurposeEdge("norm", "obj", PurposeRelationType.Serves),
                new PurposeEdge("obj", "prin", PurposeRelationType.Serves)
            });

        Assert.Equal(PurposeNodeType.Objective, graph.NodeById("obj")!.Kind);
        Assert.Null(graph.NodeById("missing"));
        Assert.Single(graph.NodesOfKind(PurposeNodeType.Norm));

        var toPrin = Assert.Single(graph.EdgesTo("prin"));
        Assert.Equal("obj", toPrin.FromNodeId);
    }

    [Fact]
    public void Norm_node_exposes_citation_abstract_nodes_do_not()
    {
        var graph = new PurposeGraph(
            new[] { Norm("norm", "Art. 47"), Abstract("obj", PurposeNodeType.Objective) },
            Array.Empty<PurposeEdge>());

        Assert.NotNull(graph.NodeById("norm")!.Citation);
        Assert.Null(graph.NodeById("obj")!.Citation);
    }

    [Fact]
    public void Conflicting_objectives_are_queryable()
    {
        var graph = new PurposeGraph(
            new[]
            {
                Norm("normA", "Art. 1"),
                Norm("normB", "Art. 2"),
                Abstract("objA", PurposeNodeType.Objective),
                Abstract("objB", PurposeNodeType.Objective)
            },
            new[]
            {
                new PurposeEdge("normA", "objA", PurposeRelationType.Serves),
                new PurposeEdge("normB", "objB", PurposeRelationType.Serves),
                new PurposeEdge("objA", "objB", PurposeRelationType.ConflictsWith)
            });

        var conflict = Assert.Single(graph.EdgesFrom("objA"));
        Assert.Equal(PurposeRelationType.ConflictsWith, conflict.Relation);
        Assert.Equal("objB", conflict.ToNodeId);
    }

    [Fact]
    public void Empty_graph_is_valid()
    {
        var graph = new PurposeGraph(Array.Empty<PurposeNode>(), Array.Empty<PurposeEdge>());

        Assert.Empty(graph.Nodes);
        Assert.Empty(graph.Edges);
        Assert.Null(graph.NodeById("anything"));
        Assert.Empty(graph.NodesOfKind(PurposeNodeType.Norm));
    }

    [Fact]
    public void Nodes_and_edges_compare_by_value()
    {
        var a = Abstract("obj", PurposeNodeType.Objective);
        var b = Abstract("obj", PurposeNodeType.Objective);
        Assert.Equal(a, b);
        Assert.NotEqual(a, a with { Id = "other" });

        var e1 = new PurposeEdge("x", "y", PurposeRelationType.Serves);
        var e2 = new PurposeEdge("x", "y", PurposeRelationType.Serves);
        Assert.Equal(e1, e2);
        Assert.NotEqual(e1, e1 with { Relation = PurposeRelationType.Supports });
    }

    [Fact]
    public void Norm_without_citation_fails_fast()
    {
        var nodes = new[] { new PurposeNode("norm", PurposeNodeType.Norm, "Norm", null) };

        Assert.Throws<InvalidOperationException>(
            () => new PurposeGraph(nodes, Array.Empty<PurposeEdge>()));
    }

    [Fact]
    public void Non_norm_with_citation_fails_fast()
    {
        var nodes = new[] { new PurposeNode("obj", PurposeNodeType.Objective, "Objective", Citation("Art. 47")) };

        Assert.Throws<InvalidOperationException>(
            () => new PurposeGraph(nodes, Array.Empty<PurposeEdge>()));
    }

    [Fact]
    public void Edge_referencing_unknown_node_fails_fast()
    {
        var nodes = new[] { Norm("norm", "Art. 47"), Abstract("obj", PurposeNodeType.Objective) };
        var edges = new[] { new PurposeEdge("norm", "ghost", PurposeRelationType.Serves) };

        Assert.Throws<InvalidOperationException>(
            () => new PurposeGraph(nodes, edges));
    }

    [Fact]
    public void Duplicate_node_id_fails_fast()
    {
        var nodes = new[]
        {
            Abstract("dup", PurposeNodeType.Objective),
            Abstract("dup", PurposeNodeType.Principle)
        };

        Assert.Throws<InvalidOperationException>(
            () => new PurposeGraph(nodes, Array.Empty<PurposeEdge>()));
    }
}
