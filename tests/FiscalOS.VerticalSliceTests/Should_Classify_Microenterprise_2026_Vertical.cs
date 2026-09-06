using FiscalOS.Runtime.Classification;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Classify_Microenterprise_2026_Vertical
{
    private static readonly Microenterprise2026Application App = new(new(100_000m, new(2025, 12, 31), 2026, new(2026, 3, 23), "Legea nr. 227/2015 Titlul III", "Art. 47", "OUG nr. 8/2026", "ANAF CJR_DEC_6319/23.03.2026", "https://static.anaf.ro/static/3/Cluj/20260402144800_cj_micro_02apr2026.pdf", true));
    private static Microenterprise2026Facts Facts(decimal revenue = 100_000m, decimal linked = 0) => new(revenue, new[] { linked }, 1m, FactStatus.True, FactStatus.True, FactStatus.True, FactStatus.True, FactStatus.True, FactStatus.True, ExcludedActivityStatus.ConfirmedNotExcluded);

    [Fact] public void Eligible_requires_all_conditions() => Assert.Equal(Microenterprise2026Outcome.Eligible, App.Evaluate(Facts()).Outcome);
    [Theory] [InlineData(600_000)] public void Revenue_exceeded_is_not_eligible(decimal revenue) => Assert.Equal(Microenterprise2026Outcome.NotEligible, App.Evaluate(Facts(revenue)).Outcome);
    [Fact] public void Linked_revenue_is_cumulative() => Assert.Equal(Microenterprise2026Outcome.NotEligible, App.Evaluate(Facts(90_000, 20_000)).Outcome);
    [Theory] [InlineData("capital")] [InlineData("dissolution")] [InlineData("employee")] [InlineData("designation")] [InlineData("statements")] public void Failed_condition_is_not_eligible(string name)
    { var f = Facts(); f = name switch { "capital" => f with { CapitalEligible = FactStatus.False }, "dissolution" => f with { NotInDissolutionLiquidation = FactStatus.False }, "employee" => f with { HasEmployee = FactStatus.False }, "designation" => f with { SingleMicroDesignation = FactStatus.False }, _ => f with { FinancialStatementsTimely = FactStatus.False } }; Assert.Equal(Microenterprise2026Outcome.NotEligible, App.Evaluate(f).Outcome); }
    [Fact] public void Unknown_fact_is_insufficient() => Assert.Equal(Microenterprise2026Outcome.InsufficientInformation, App.Evaluate(Facts() with { CapitalEligible = FactStatus.Unknown }).Outcome);
    [Fact] public void Unknown_exclusion_status_is_unsupported() => Assert.Equal(Microenterprise2026Outcome.Unsupported, App.Evaluate(Facts() with { ExcludedActivity = ExcludedActivityStatus.Unknown }).Outcome);
    [Fact] public void Knowledge_is_curated_and_temporal() { var r = App.Evaluate(Facts()); Assert.True(r.Knowledge.Curated); Assert.Equal(2026, r.Knowledge.FiscalYear); Assert.Contains("Art. 47", r.Explanation); }
}
