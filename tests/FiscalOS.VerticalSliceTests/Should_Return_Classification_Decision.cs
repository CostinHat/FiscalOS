using FiscalOS.Core;
using FiscalOS.Core.Classification;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Return_Classification_Decision
{
    private static FiscalSubject EligibleSubject() =>
        new()
        {
            Name = "Test Company",
            FiscalCode = new FiscalCode("TEST"),
            TaxIdentificationNumber = new TaxIdentificationNumber("12345678"),
            Revenue = 320_000m,
            EmployeeCount = 3
        };

    private static ClassificationEngine Engine(params ClassificationRule[] rules) =>
        new(new DefaultRuleRegistry(rules));

    [Fact]
    public async Task Engine_returns_a_decision_wrapping_the_result()
    {
        var engine = Engine(new MicroenterpriseClassificationRule());

        var decision = await engine.ClassifyAsync(EligibleSubject());

        Assert.IsType<ClassificationDecision>(decision);
        Assert.NotNull(decision.Result);
        Assert.IsType<ClassificationResult>(decision.Result);
        Assert.Equal("Microenterprise", decision.Result.Category);
    }

    [Fact]
    public async Task Decision_captures_the_winning_rule_id()
    {
        var engine = Engine(new MicroenterpriseClassificationRule());

        var decision = await engine.ClassifyAsync(EligibleSubject());

        Assert.Equal("MICROENTERPRISE_ELIGIBILITY", decision.WinningRuleId);
    }

    [Fact]
    public async Task Decision_has_no_winning_rule_id_when_nothing_passes()
    {
        var subject = new FiscalSubject { Revenue = 600_000m, EmployeeCount = 3 };
        var engine = Engine(new MicroenterpriseClassificationRule());

        var decision = await engine.ClassifyAsync(subject);

        Assert.Null(decision.WinningRuleId);
        Assert.Equal("Unclassified", decision.Result.Category);
    }

    [Fact]
    public async Task Decision_preserves_all_evaluated_rule_results()
    {
        var engine = Engine(
            new MicroenterpriseClassificationRule(),
            new AlwaysPassRule());

        var decision = await engine.ClassifyAsync(EligibleSubject());

        Assert.Equal(2, decision.RuleResults.Count);
        Assert.Contains(decision.RuleResults, r => r.RuleId == "MICROENTERPRISE_ELIGIBILITY");
        Assert.Contains(decision.RuleResults, r => r.RuleId == "ALWAYS_PASS");
    }
}
