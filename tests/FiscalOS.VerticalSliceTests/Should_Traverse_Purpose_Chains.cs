using System;
using System.Linq;
using FiscalOS.Core;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using FiscalOS.Runtime.Evaluation;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Traverse_Purpose_Chains
{
    private static LegalCitation Citation(string article) =>
        new(LegalSourceType.Law, "Legea 227/2015", article, SpecificityLevel.Specific, new DateOnly(2024, 1, 1));

    private sealed class CitingStubRule : ClassificationRule
    {
        private readonly IReadOnlyList<LegalCitation> _citations;

        public CitingStubRule(params LegalCitation[] citations) => _citations = citations;

        public string RuleId => "STUB_RULE";

        public string Description => "Test stub that emits legal citations.";

        public int Priority => 100;

        public RuleEvaluationResult Evaluate(ClassificationContext context) =>
            new(RuleId, true, "stub passed", "StubCategory") { Citations = _citations };
    }

    private static async Task<DecisionExplanation> ExplanationFor(params LegalCitation[] citations)
    {
        var engine = new ClassificationEngine(
            new DefaultRuleRegistry(new ClassificationRule[] { new CitingStubRule(citations) }));
        var decision = await engine.ClassifyAsync(new FiscalSubject());
        return decision.Explanation;
    }

    private static PurposeNode Norm(string id, LegalCitation citation) =>
        new(id, PurposeNodeType.Norm, $"Norm {id}", citation);

    private static PurposeNode Abstract(string id, PurposeNodeType kind) =>
        new(id, kind, $"{kind} {id}", null);

    private static PurposeEdge Serves(string from, string to) =>
        new(from, to, PurposeRelationType.Serves);

    [Fact]
    public async Task Traverses_a_full_teleological_ladder()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[]
            {
                Norm("norm", citation),
                Abstract("obj", PurposeNodeType.Objective),
                Abstract("prin", PurposeNodeType.Principle),
                Abstract("val", PurposeNodeType.ProtectedValue)
            },
            new[] { Serves("norm", "obj"), Serves("obj", "prin"), Serves("prin", "val") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var chain = Assert.Single(aware.GoverningPurposeChains());

        Assert.Equal(citation, chain.GoverningCitation);
        Assert.Equal(new[] { "norm", "obj", "prin", "val" }, chain.Nodes.Select(n => n.Id));
        Assert.Equal(4, chain.Depth);
        Assert.Equal("val", chain.Leaf.Id);
    }

    [Fact]
    public async Task Norm_without_serves_edge_yields_a_single_node_chain()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(new[] { Norm("norm", citation) }, Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, graph);

        var chain = Assert.Single(aware.GoverningPurposeChains());

        Assert.Equal(new[] { "norm" }, chain.Nodes.Select(n => n.Id));
        Assert.Equal(1, chain.Depth);
    }

    [Fact]
    public async Task Partial_chain_stops_where_serves_edges_end()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[] { Norm("norm", citation), Abstract("obj", PurposeNodeType.Objective) },
            new[] { Serves("norm", "obj") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var chain = Assert.Single(aware.GoverningPurposeChains());

        Assert.Equal(new[] { "norm", "obj" }, chain.Nodes.Select(n => n.Id));
    }

    [Fact]
    public async Task Branching_produces_one_chain_per_path_in_edge_order()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[]
            {
                Norm("norm", citation),
                Abstract("objA", PurposeNodeType.Objective),
                Abstract("objB", PurposeNodeType.Objective)
            },
            new[] { Serves("norm", "objA"), Serves("norm", "objB") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var chains = aware.GoverningPurposeChains();

        Assert.Equal(2, chains.Count);
        Assert.Equal(new[] { "norm", "objA" }, chains[0].Nodes.Select(n => n.Id));
        Assert.Equal(new[] { "norm", "objB" }, chains[1].Nodes.Select(n => n.Id));
    }

    [Fact]
    public async Task Cycle_terminates_silently_without_revisiting()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[] { Norm("norm", citation), Abstract("obj", PurposeNodeType.Objective) },
            new[] { Serves("norm", "obj"), Serves("obj", "norm") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var chain = Assert.Single(aware.GoverningPurposeChains());

        Assert.Equal(new[] { "norm", "obj" }, chain.Nodes.Select(n => n.Id));
        Assert.Equal(chain.Nodes.Select(n => n.Id), chain.Nodes.Select(n => n.Id).Distinct());
    }

    [Fact]
    public async Task Non_serves_edges_are_not_followed()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[] { Norm("norm", citation), Abstract("obj", PurposeNodeType.Objective) },
            new[] { new PurposeEdge("norm", "obj", PurposeRelationType.Supports) });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var chain = Assert.Single(aware.GoverningPurposeChains());

        Assert.Equal(new[] { "norm" }, chain.Nodes.Select(n => n.Id));
    }

    [Fact]
    public async Task No_purpose_context_yields_no_chains()
    {
        var explanation = await ExplanationFor(Citation("Art. 1"));
        var emptyGraph = new PurposeGraph(Array.Empty<PurposeNode>(), Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, emptyGraph);

        Assert.Empty(aware.GoverningPurposeChains());
    }

    [Fact]
    public async Task Multiple_governing_citations_group_chains_per_citation()
    {
        var first = Citation("Art. 1");
        var second = Citation("Art. 2");
        // Two same-tier citations tie -> unresolved basis -> both are governing.
        var explanation = await ExplanationFor(first, second);
        var graph = new PurposeGraph(
            new[]
            {
                Norm("norm1", first),
                Norm("norm2", second),
                Abstract("objA", PurposeNodeType.Objective),
                Abstract("objB", PurposeNodeType.Objective)
            },
            new[] { Serves("norm1", "objA"), Serves("norm2", "objB") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var chains = aware.GoverningPurposeChains();

        Assert.Equal(2, chains.Count);
        Assert.Equal(first, chains[0].GoverningCitation);
        Assert.Equal(new[] { "norm1", "objA" }, chains[0].Nodes.Select(n => n.Id));
        Assert.Equal(second, chains[1].GoverningCitation);
        Assert.Equal(new[] { "norm2", "objB" }, chains[1].Nodes.Select(n => n.Id));
    }

    [Fact]
    public async Task Traversal_is_deterministic()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[]
            {
                Norm("norm", citation),
                Abstract("obj", PurposeNodeType.Objective),
                Abstract("prin", PurposeNodeType.Principle)
            },
            new[] { Serves("norm", "obj"), Serves("obj", "prin") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var first = aware.GoverningPurposeChains();
        var second = aware.GoverningPurposeChains();

        Assert.Equal(
            first.Select(c => string.Join(">", c.Nodes.Select(n => n.Id))),
            second.Select(c => string.Join(">", c.Nodes.Select(n => n.Id))));
    }

    [Fact]
    public async Task Traversal_does_not_alter_governing_law()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(new[] { Norm("norm", citation) }, Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, graph);

        _ = aware.GoverningPurposeChains();

        Assert.Equal(new[] { citation }, aware.Explanation.LegalBasis.GoverningCitations);
    }
}
