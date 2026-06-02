using FiscalOS.Core;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Classify_Microenterprise
{
    private static FiscalSubject Subject(decimal revenue, int employeeCount) =>
        new()
        {
            Name = "Test Company",
            FiscalCode = new FiscalCode("TEST"),
            TaxIdentificationNumber = new TaxIdentificationNumber("12345678"),
            Revenue = revenue,
            EmployeeCount = employeeCount
        };

    [Fact]
    public void Rule_passes_and_assigns_category_for_eligible_subject()
    {
        var rule = new MicroenterpriseClassificationRule();

        var result = rule.Evaluate(
            new ClassificationContext(Subject(320_000m, 3)));

        Assert.True(result.Passed);
        Assert.Equal("Microenterprise", result.Category);
        Assert.Equal("MICROENTERPRISE_ELIGIBILITY", result.RuleId);
    }

    [Fact]
    public void Rule_fails_without_category_when_revenue_exceeds_threshold()
    {
        var rule = new MicroenterpriseClassificationRule();

        var result = rule.Evaluate(
            new ClassificationContext(Subject(600_000m, 3)));

        Assert.False(result.Passed);
        Assert.Null(result.Category);
    }

    [Fact]
    public void Rule_fails_without_category_when_no_employees()
    {
        var rule = new MicroenterpriseClassificationRule();

        var result = rule.Evaluate(
            new ClassificationContext(Subject(320_000m, 0)));

        Assert.False(result.Passed);
        Assert.Null(result.Category);
    }

    [Fact]
    public async Task Engine_produces_microenterprise_category_for_eligible_subject()
    {
        var registry = new DefaultRuleRegistry(
            new ClassificationRule[]
            {
                new MicroenterpriseClassificationRule()
            });

        var engine = new ClassificationEngine(registry);

        var decision = await engine.ClassifyAsync(Subject(320_000m, 3));

        Assert.Equal("Microenterprise", decision.Result.Category);
        Assert.Contains("MICROENTERPRISE_ELIGIBILITY", decision.Result.Explanation);
    }

    [Fact]
    public async Task Engine_falls_back_to_unclassified_for_ineligible_subject()
    {
        var registry = new DefaultRuleRegistry(
            new ClassificationRule[]
            {
                new MicroenterpriseClassificationRule()
            });

        var engine = new ClassificationEngine(registry);

        var decision = await engine.ClassifyAsync(Subject(600_000m, 3));

        Assert.Equal("Unclassified", decision.Result.Category);
    }

    [Fact]
    public async Task Higher_priority_microenterprise_rule_wins_over_always_pass()
    {
        var registry = new DefaultRuleRegistry(
            new ClassificationRule[]
            {
                new AlwaysPassRule(),
                new MicroenterpriseClassificationRule()
            });

        var engine = new ClassificationEngine(registry);

        var decision = await engine.ClassifyAsync(Subject(320_000m, 3));

        Assert.Equal("Microenterprise", decision.Result.Category);
    }

    [Fact]
    public void Eligible_subject_emits_the_curated_governing_citation()
    {
        var rule = new MicroenterpriseClassificationRule();

        var result = rule.Evaluate(new ClassificationContext(Subject(320_000m, 3)));

        var citation = Assert.Single(result.Citations);
        Assert.Equal(LegalSourceType.FiscalCode, citation.SourceType);
        Assert.Equal("Legea 227/2015", citation.SourceReference);
        Assert.Equal("Art. 47", citation.Article);
        Assert.Equal("RO", citation.Jurisdiction.Value);
    }

    [Fact]
    public void Ineligible_subject_emits_no_citation()
    {
        var rule = new MicroenterpriseClassificationRule();

        var result = rule.Evaluate(new ClassificationContext(Subject(600_000m, 3)));

        Assert.Empty(result.Citations);
    }

    [Fact]
    public async Task Engine_surfaces_the_curated_citation_as_governing_legal_basis()
    {
        var registry = new DefaultRuleRegistry(
            new ClassificationRule[] { new MicroenterpriseClassificationRule() });
        var engine = new ClassificationEngine(registry);

        var decision = await engine.ClassifyAsync(Subject(320_000m, 3));

        var citation = Assert.Single(decision.Explanation.LegalBasis.GoverningCitations);
        Assert.Equal("Art. 47", citation.Article);
        Assert.Equal("RO", citation.Jurisdiction.Value);
    }

    [Fact]
    public void Threshold_and_citation_come_from_the_same_curated_definition()
    {
        var rule = new MicroenterpriseClassificationRule();

        // The eligibility boundary is driven by the curated regime threshold.
        var atThreshold = rule.Evaluate(new ClassificationContext(
            Subject(MicroenterpriseRegime.RevenueThreshold, MicroenterpriseRegime.MinimumEmployeeCount)));
        var aboveThreshold = rule.Evaluate(new ClassificationContext(
            Subject(MicroenterpriseRegime.RevenueThreshold + 1m, MicroenterpriseRegime.MinimumEmployeeCount)));

        Assert.True(atThreshold.Passed);
        Assert.False(aboveThreshold.Passed);

        // The emitted citation is the same curated definition's citation.
        Assert.Equal(MicroenterpriseRegime.Citation, Assert.Single(atThreshold.Citations));
    }
}
