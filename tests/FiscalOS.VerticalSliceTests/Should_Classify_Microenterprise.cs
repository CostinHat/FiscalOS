using FiscalOS.Core;
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
}
