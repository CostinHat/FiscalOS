using System;
using System.Linq;
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

    private static PurposeNode Norm(string id, string description, LegalCitation citation) =>
        new(id, PurposeNodeType.Norm, description, citation);

    private static PurposeNode Abstract(string id, PurposeNodeType kind, string description) =>
        new(id, kind, description, null);

    private static PurposeEdge Serves(string from, string to) =>
        new(from, to, PurposeRelationType.Serves);

    [Fact]
    public async Task Renders_a_full_teleological_chain()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[]
            {
                Norm("norm", "X", citation),
                Abstract("obj", PurposeNodeType.Objective, "Y"),
                Abstract("prin", PurposeNodeType.Principle, "Z"),
                Abstract("val", PurposeNodeType.ProtectedValue, "W")
            },
            new[] { Serves("norm", "obj"), Serves("obj", "prin"), Serves("prin", "val") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var narrative = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Contains(
            "Purpose chain: Norm \"X\" serves Objective \"Y\" serves Principle \"Z\" serves ProtectedValue \"W\".",
            narrative.Lines);
        // Base lines preserved.
        Assert.Contains("Decision: StubCategory.", narrative.Text);
        Assert.Contains("resolved to 1 governing citation(s).", narrative.Text);
    }

    [Fact]
    public async Task Renders_a_single_node_chain()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[] { Norm("norm", "X", citation) },
            Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, graph);

        var narrative = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Contains("Purpose chain: Norm \"X\".", narrative.Lines);
        // The FOS-0048 flat purpose line is no longer produced.
        Assert.DoesNotContain("Purpose: X.", narrative.Lines);
    }

    [Fact]
    public async Task Renders_a_partial_chain()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[] { Norm("norm", "X", citation), Abstract("obj", PurposeNodeType.Objective, "Y") },
            new[] { Serves("norm", "obj") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var narrative = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Contains("Purpose chain: Norm \"X\" serves Objective \"Y\".", narrative.Lines);
    }

    [Fact]
    public async Task Renders_one_line_per_branching_chain_in_order()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[]
            {
                Norm("norm", "X", citation),
                Abstract("objA", PurposeNodeType.Objective, "A"),
                Abstract("objB", PurposeNodeType.Objective, "B")
            },
            new[] { Serves("norm", "objA"), Serves("norm", "objB") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var chainLines = PurposeAwareNarrativeProjector.Project(aware)
            .Lines.Where(line => line.StartsWith("Purpose chain:")).ToList();

        Assert.Equal(
            new[]
            {
                "Purpose chain: Norm \"X\" serves Objective \"A\".",
                "Purpose chain: Norm \"X\" serves Objective \"B\"."
            },
            chainLines);
    }

    [Fact]
    public async Task Base_lines_come_first_then_chain_lines()
    {
        var citation = Citation("Art. 1");
        var explanation = await ExplanationFor(citation);
        var graph = new PurposeGraph(
            new[] { Norm("norm", "X", citation) },
            Array.Empty<PurposeEdge>());
        var aware = new PurposeAwareExplanation(explanation, graph);

        var baseNarrative = ExplanationNarrativeProjector.Project(explanation);
        var narrative = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Equal(baseNarrative.Lines, narrative.Lines.Take(baseNarrative.Lines.Count));
        Assert.Equal("Purpose chain: Norm \"X\".", narrative.Lines[^1]);
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
    public async Task Unmatched_purpose_nodes_add_no_chain_lines()
    {
        var explanation = await ExplanationFor(Citation("Art. 1"));
        var graph = new PurposeGraph(
            new[] { Norm("norm", "X", Citation("Art. 99")) },
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
            new[]
            {
                Norm("norm", "X", citation),
                Abstract("obj", PurposeNodeType.Objective, "Y")
            },
            new[] { Serves("norm", "obj") });
        var aware = new PurposeAwareExplanation(explanation, graph);

        var first = PurposeAwareNarrativeProjector.Project(aware);
        var second = PurposeAwareNarrativeProjector.Project(aware);

        Assert.Equal(first.Lines, second.Lines);
        Assert.Equal(first.Text, second.Text);
    }
}
