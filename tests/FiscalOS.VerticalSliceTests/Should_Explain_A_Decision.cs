using System;
using FiscalOS.Core;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using FiscalOS.Runtime.Evaluation;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Explain_A_Decision
{
    private static LegalCitation Citation(string article) =>
        new(LegalSourceType.Law, "Legea 227/2015", article, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), new JurisdictionId("RO"));

    private static ClassificationEngine Engine(params ClassificationRule[] rules) =>
        new(new DefaultRuleRegistry(rules));

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

    [Fact]
    public async Task Decision_exposes_a_single_explanation_surface()
    {
        var engine = Engine(new CitingStubRule(Citation("Art. 1")));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.NotNull(decision.Explanation);
        Assert.NotNull(decision.Explanation.LegalBasis);
        Assert.NotNull(decision.Explanation.AuditGraph);
    }

    [Fact]
    public async Task Explanation_aggregates_the_legal_basis_and_audit_graph()
    {
        var citation = Citation("Art. 1");
        var engine = Engine(new CitingStubRule(citation));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.True(decision.Explanation.HasLegalBasis);
        Assert.Equal(new[] { citation }, decision.Explanation.LegalBasis.GoverningCitations);
        Assert.NotNull(decision.Explanation.AuditGraph.NodeById("decision"));
    }

    [Fact]
    public async Task Explanation_flags_an_unresolved_legal_conflict()
    {
        var a = Citation("Art. 1");
        var b = Citation("Art. 2");
        var engine = Engine(new CitingStubRule(a, b));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.True(decision.Explanation.HasUnresolvedLegalConflict);
        Assert.True(decision.Explanation.HasLegalBasis);
    }

    [Fact]
    public async Task Explanation_reports_no_legal_basis_when_winner_has_no_citations()
    {
        var engine = Engine(new CitingStubRule());

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.False(decision.Explanation.HasLegalBasis);
        Assert.False(decision.Explanation.HasUnresolvedLegalConflict);
        Assert.True(decision.Explanation.LegalBasis.IsEmpty);
    }

    [Fact]
    public void Explanation_compares_by_value()
    {
        var basis = DecisionLegalBasis.Resolve(new[] { Citation("Art. 1") });
        var graph = new AuditGraph(Array.Empty<AuditNode>(), Array.Empty<AuditEdge>());

        var a = new DecisionExplanation(basis, graph);
        var b = new DecisionExplanation(basis, graph);

        Assert.Equal(a, b);
    }
}
