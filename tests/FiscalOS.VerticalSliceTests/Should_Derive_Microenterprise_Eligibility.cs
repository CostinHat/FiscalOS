using Xunit;
using FiscalOS.Runtime;
using FiscalOS.Core;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Derive_Microenterprise_Eligibility
{
    [Fact]
    public void Execute()
    {
        var revenue = 320_000m;
        var employeeCount = 3;

        var subject = new FiscalSubject
{
    FiscalCode = new FiscalCode("TEST"),
    Name = "Test Company",
    Revenue = revenue,
    EmployeeCount = employeeCount,
    TaxIdentificationNumber = new TaxIdentificationNumber("12345678")
};
var rule = new MicroenterpriseEligibilityRule();

var result = rule.Evaluate(subject);

        Assert.True(result.HasAssertion("MICROENTERPRISE_ELIGIBLE", true));
        Assert.NotNull(result.Explanation);
        Assert.Contains("Revenue=320000", result.Explanation!.Text);
        Assert.Contains("EmployeeCount=3", result.Explanation.Text);
    }
}
