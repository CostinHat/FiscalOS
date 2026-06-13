using System.Linq;
using System;
using FiscalOS.Core;
using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification.Rules;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Evaluation;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Enrich_Explanation_With_Purpose
{
    private static LegalCitation Citation(string article) =>
        new(LegalSourceType.Law, "Legea 227/2015", article, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), new JurisdictionId("RO"));

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

    private static PurposeGraph GraphWithNorm(string id, LegalCitation citation) =>
        new(new[] { new PurposeNode(id, PurposeNodeType.Norm, $"Norm {id}", citation) },
            Array.Empty<PurposeEdge>());

    [Fact]
    public async Task Composes_explanation_and_purpose_graph()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = GraphWithNorm("norm", citation);

        var aware = new PurposeAwareExplanation(explanation, graph);

        Assert.Same(explanation, aware.Explanation);
        Assert.Same(graph, aware.PurposeGraph);
        Assert.True(aware.HasPurposeContext);
    }

    [Fact]
    public async Task Links_governing_citation_to_matching_norm_node()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = GraphWithNorm("norm", citation);

        var aware = new PurposeAwareExplanation(explanation, graph);

        var reference = Assert.Single(aware.GoverningPurposes());
        Assert.Equal(citation, reference.GoverningCitation);
        Assert.Equal("norm", reference.PurposeNode.Id);
        Assert.Equal(PurposeNodeType.Norm, reference.PurposeNode.Kind);
    }

    [Fact]
    public async Task Omits_governing_citations_without_a_matching_norm_node()
    {
        var governing = Citation("Art. 1");
        var unrelated = Citation("Art. 99");
        var explanation = await ExplanationFor(governing);
        var graph = GraphWithNorm("norm", unrelated);

        var aware = new PurposeAwareExplanation(explanation, graph);

        Assert.Empty(aware.GoverningPurposes());
    }

    [Fact]
    public async Task Empty_legal_basis_yields_no_purpose_references()
    {
        var eligible = new FiscalSubject { Revenue = 320_000m, EmployeeCount = 3 };
        var engine = new ClassificationEngine(
            new DefaultRuleRegistry(new ClassificationRule[] { new MicroenterpriseClassificationRule() }));
        var decision = await engine.ClassifyAsync(eligible);

        var aware = new PurposeAwareExplanation(decision.Explanation, GraphWithNorm("norm", Citation("Art. 1")));

        Assert.Empty(aware.GoverningPurposes());
    }

    [Fact]
    public async Task Empty_purpose_graph_yields_no_references_and_no_context()
    {
        var explanation = await ExplanationFor(Citation("Art. 1"));
        var emptyGraph = new PurposeGraph(Array.Empty<PurposeNode>(), Array.Empty<PurposeEdge>());

        var aware = new PurposeAwareExplanation(explanation, emptyGraph);

        Assert.False(aware.HasPurposeContext);
        Assert.Empty(aware.GoverningPurposes());
    }

    [Fact]
    public async Task Purpose_does_not_alter_governing_law()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = GraphWithNorm("norm", citation);

        var aware = new PurposeAwareExplanation(explanation, graph);

        Assert.Equal(
            explanation.LegalBasis.GoverningCitations,
            aware.Explanation.LegalBasis.GoverningCitations);
    }

    [Fact]
    public void Purpose_reference_compares_by_value()
    {
        var citation = Citation("Art. 1");
        var node = new PurposeNode("norm", PurposeNodeType.Norm, "Norm norm", citation);

        var a = new PurposeReference(citation, node);
        var b = new PurposeReference(citation, node);

        Assert.Equal(a, b);
    }
}
