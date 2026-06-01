using System;
using FiscalOS.Core;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using FiscalOS.Runtime.Evaluation;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Project_Explanation_Narrative
{
    private static LegalCitation Citation(string article) =>
        new(LegalSourceType.Law, "Legea 227/2015", article, SpecificityLevel.Specific, new DateOnly(2024, 1, 1));

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

    private static async Task<ExplanationNarrative> ProjectFor(
        ClassificationEngine engine,
        FiscalSubject subject)
    {
        var decision = await engine.ClassifyAsync(subject);
        return ExplanationNarrativeProjector.Project(decision.Explanation);
    }

    [Fact]
    public async Task Resolved_basis_narrative_states_category_and_governing_citations()
    {
        var narrative = await ProjectFor(
            Engine(new CitingStubRule(Citation("Art. 1"))),
            new FiscalSubject());

        Assert.Contains("Decision: StubCategory.", narrative.Text);
        Assert.Contains("resolved to 1 governing citation(s).", narrative.Text);
    }

    [Fact]
    public async Task Unresolved_basis_narrative_states_the_conflict()
    {
        var narrative = await ProjectFor(
            Engine(new CitingStubRule(Citation("Art. 1"), Citation("Art. 2"))),
            new FiscalSubject());

        Assert.Contains("unresolved conflict among 2 competing citation(s).", narrative.Text);
    }

    [Fact]
    public async Task Empty_basis_narrative_states_no_legal_basis()
    {
        var eligible = new FiscalSubject { Revenue = 320_000m, EmployeeCount = 3 };

        var narrative = await ProjectFor(
            Engine(new MicroenterpriseClassificationRule()),
            eligible);

        Assert.Contains("Decision: Microenterprise.", narrative.Text);
        Assert.Contains("Legal basis: none established.", narrative.Text);
    }

    [Fact]
    public async Task Unclassified_decision_narrative_reports_unclassified()
    {
        var ineligible = new FiscalSubject { Revenue = 600_000m, EmployeeCount = 3 };

        var narrative = await ProjectFor(
            Engine(new MicroenterpriseClassificationRule()),
            ineligible);

        Assert.Contains("Decision: Unclassified.", narrative.Text);
        Assert.Contains("Legal basis: none established.", narrative.Text);
    }

    [Fact]
    public async Task Provenance_line_summarizes_rules_and_conflict()
    {
        var withConflict = await ProjectFor(
            Engine(new CitingStubRule(Citation("Art. 1"))),
            new FiscalSubject());
        Assert.Contains("Provenance: 1 rule(s) evaluated, conflict resolution applied.", withConflict.Text);

        var noConflict = await ProjectFor(
            Engine(new MicroenterpriseClassificationRule()),
            new FiscalSubject { Revenue = 320_000m, EmployeeCount = 3 });
        Assert.Contains("Provenance: 1 rule(s) evaluated.", noConflict.Text);
    }

    [Fact]
    public async Task Projection_is_deterministic()
    {
        var decision = await Engine(new CitingStubRule(Citation("Art. 1"))).ClassifyAsync(new FiscalSubject());

        var first = ExplanationNarrativeProjector.Project(decision.Explanation);
        var second = ExplanationNarrativeProjector.Project(decision.Explanation);

        Assert.Equal(first.Lines, second.Lines);
        Assert.Equal(first.Text, second.Text);
    }

    [Fact]
    public void Outcome_falls_back_to_unknown_when_no_decision_node()
    {
        var explanation = new DecisionExplanation(
            DecisionLegalBasis.Resolve(Array.Empty<LegalCitation>()),
            new AuditGraph(Array.Empty<AuditNode>(), Array.Empty<AuditEdge>()));

        var narrative = ExplanationNarrativeProjector.Project(explanation);

        Assert.Contains("Decision: Unknown.", narrative.Text);
        Assert.Contains("Legal basis: none established.", narrative.Text);
        Assert.Contains("Provenance: 0 rule(s) evaluated.", narrative.Text);
    }
}
