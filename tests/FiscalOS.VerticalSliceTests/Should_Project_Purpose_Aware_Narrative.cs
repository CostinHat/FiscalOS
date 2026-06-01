using System;
using FiscalOS.Core;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using FiscalOS.Runtime.Evaluation;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Project_Purpose_Aware_Narrative
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

    private static PurposeNode Norm(string id, string description, LegalCitation citation) =>
        new(id, PurposeNodeType.Norm, description, citation);

    [Fact]
    public async Task Appends_a_purpose_line_per_matching_reference()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[] { Norm("norm", "Encourage small business formation", citation) },
            Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, graph);

        var narrative = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Contains("Purpose: Encourage small business formation.", narrative.Lines);
        // Base lines are preserved.
        Assert.Contains("Decision: StubCategory.", narrative.Text);
        Assert.Contains("resolved to 1 governing citation(s).", narrative.Text);
    }

    [Fact]
    public async Task Base_lines_come_first_then_purpose_lines()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[] { Norm("norm", "Some purpose", citation) },
            Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, graph);

        var baseNarrative = ExplanationNarrativeProjector.Project(explanation);
        var narrative = PurposeAwareNarrativeProjector.Project(aware);

        // The purpose-aware narrative is the base narrative followed by purpose lines.
        Assert.Equal(baseNarrative.Lines, narrative.Lines.Take(baseNarrative.Lines.Count));
        Assert.Equal("Purpose: Some purpose.", narrative.Lines[^1]);
    }

    [Fact]
    public async Task Multiple_references_become_multiple_purpose_lines()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[]
            {
                Norm("normA", "Purpose A", citation),
                Norm("normB", "Purpose B", citation)
            },
            Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, graph);

        var narrative = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Contains("Purpose: Purpose A.", narrative.Lines);
        Assert.Contains("Purpose: Purpose B.", narrative.Lines);
    }

    [Fact]
    public async Task No_purpose_context_yields_the_base_narrative_unchanged()
    {
        var explanation = await ExplanationFor(Citation("Art. 1"));
        var emptyGraph = new PurposeGraph(Array.Empty<PurposeNode>(), Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, emptyGraph);

        var baseNarrative = ExplanationNarrativeProjector.Project(explanation);
        var narrative = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Equal(baseNarrative.Lines, narrative.Lines);
    }

    [Fact]
    public async Task Unmatched_purpose_nodes_add_no_lines()
    {
        var explanation = await ExplanationFor(Citation("Art. 1"));
        var graph = new PurposeGraph(
            new[] { Norm("norm", "Unrelated purpose", Citation("Art. 99")) },
            Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, graph);

        var baseNarrative = ExplanationNarrativeProjector.Project(explanation);
        var narrative = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Equal(baseNarrative.Lines, narrative.Lines);
    }

    [Fact]
    public async Task Projection_is_deterministic()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[] { Norm("norm", "Stable purpose", citation) },
            Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, graph);

        var first = PurposeAwareNarrativeProjector.Project(aware);
        var second = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Equal(first.Lines, second.Lines);
        Assert.Equal(first.Text, second.Text);
    }
}
