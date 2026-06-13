using System;
using FiscalOS.Core;
using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification.Rules;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Evaluation;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Attach_Legal_Basis_To_Decision
{
    private static LegalCitation Citation(
        LegalSourceType sourceType,
        SpecificityLevel specificity,
        DateOnly effectiveDate,
        string article) =>
        new(sourceType, "Legea 227/2015", article, specificity, effectiveDate, new JurisdictionId("RO"));

    private static ClassificationEngine Engine(params ClassificationRule[] rules) =>
        new(new DefaultRuleRegistry(rules));


    private sealed class StubLegalBasisResolver : ILegalBasisResolver
    {
        private readonly LegalCitation _governingCitation;

        public StubLegalBasisResolver(LegalCitation governingCitation) =>
            _governingCitation = governingCitation;

        public IReadOnlyList<LegalCitation>? SeenCitations { get; private set; }

        public DecisionLegalBasis Resolve(IReadOnlyList<LegalCitation> consideredCitations)
        {
            SeenCitations = consideredCitations;

            return new DecisionLegalBasis(
                consideredCitations,
                new ConflictResolutionResult(new[] { _governingCitation }));
        }
    }
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
    public async Task Engine_uses_configured_legal_basis_resolver()
    {
        var first = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 1");
        var second = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 2");
        var resolver = new StubLegalBasisResolver(second);
        var engine = new ClassificationEngine(
            new DefaultRuleRegistry(new ClassificationRule[] { new CitingStubRule(first, second) }),
            resolver);

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.Equal(new[] { first, second }, resolver.SeenCitations);
        Assert.Equal(new[] { second }, decision.Explanation.LegalBasis.GoverningCitations);
    }
    [Fact]
    public async Task Single_citation_becomes_a_resolved_legal_basis()
    {
        var citation = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 1");
        var engine = Engine(new CitingStubRule(citation));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.True(decision.Explanation.LegalBasis.IsResolved);
        Assert.False(decision.Explanation.HasUnresolvedLegalConflict);
        Assert.Equal(new[] { citation }, decision.Explanation.LegalBasis.ConsideredCitations);
        Assert.Equal(new[] { citation }, decision.Explanation.LegalBasis.GoverningCitations);
    }

    [Fact]
    public async Task Competing_citations_resolve_to_the_higher_authority()
    {
        var law = Citation(LegalSourceType.Law, SpecificityLevel.General, new DateOnly(2018, 1, 1), "Art. 1");
        var order = Citation(LegalSourceType.ANAFOrder, SpecificityLevel.General, new DateOnly(2025, 1, 1), "Art. 2");
        var engine = Engine(new CitingStubRule(order, law));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.True(decision.Explanation.LegalBasis.IsResolved);
        Assert.Equal(new[] { law }, decision.Explanation.LegalBasis.GoverningCitations);
    }

    [Fact]
    public async Task Unresolved_legal_conflict_is_flagged_but_does_not_block_classification()
    {
        var a = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 1");
        var b = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 2");
        var engine = Engine(new CitingStubRule(a, b));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.True(decision.Explanation.HasUnresolvedLegalConflict);
        Assert.True(decision.Explanation.LegalBasis.IsUnresolved);
        Assert.Equal(new[] { a, b }, decision.Explanation.LegalBasis.GoverningCitations);
        Assert.Equal("StubCategory", decision.Result.Category);
    }

    [Fact]
    public async Task No_winning_rule_yields_an_empty_legal_basis()
    {
        var ineligible = new FiscalSubject { Revenue = 600_000m, EmployeeCount = 3 };
        var engine = Engine(new MicroenterpriseClassificationRule());

        var decision = await engine.ClassifyAsync(ineligible);

        Assert.Null(decision.WinningRuleId);
        Assert.True(decision.Explanation.LegalBasis.IsEmpty);
        Assert.False(decision.Explanation.HasUnresolvedLegalConflict);
    }

    [Fact]
    public async Task Winning_rule_without_citations_yields_an_empty_legal_basis()
    {
        var engine = Engine(new CitingStubRule());

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.Equal("STUB_RULE", decision.WinningRuleId);
        Assert.True(decision.Explanation.LegalBasis.IsEmpty);
    }

    [Fact]
    public async Task Malformed_citation_on_the_winning_rule_fails_fast()
    {
        var incomplete = Citation(LegalSourceType.Law, SpecificityLevel.Specific, default, "Art. 1");
        var engine = Engine(new CitingStubRule(incomplete));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => engine.ClassifyAsync(new FiscalSubject()));
    }

    [Fact]
    public async Task Governing_citations_preserve_jurisdiction()
    {
        var citation = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 1");
        var engine = Engine(new CitingStubRule(citation));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        var governing = Assert.Single(decision.Explanation.LegalBasis.GoverningCitations);
        Assert.Equal("RO", governing.Jurisdiction.Value);
    }
}
