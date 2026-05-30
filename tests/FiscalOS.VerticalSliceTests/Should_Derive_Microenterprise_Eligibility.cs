using Xunit;
using FiscalOS.Runtime;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Derive_Microenterprise_Eligibility
{
    [Fact]
    public void Execute()
    {
        var revenue = 320_000m;
        var employeeCount = 3;

        var result = EvaluationEngine.Evaluate(revenue, employeeCount);

        Assert.True(result.HasAssertion("MICROENTERPRISE_ELIGIBLE", true));
        Assert.NotNull(result.Explanation);
        Assert.Contains("Revenue=320000", result.Explanation!.Text);
        Assert.Contains("EmployeeCount=3", result.Explanation.Text);
    }
}
