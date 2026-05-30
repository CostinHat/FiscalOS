using FiscalOS.Core;
using FiscalOS.Core.Rules;
namespace FiscalOS.Api.Endpoints;

public static class MicroenterpriseEligibilityEndpoint
{
    public static void MapMicroenterpriseEligibilityEndpoint(this WebApplication app)
    {
        app.MapPost("/microenterprise-eligibility", (MicroenterpriseRequest request) =>
        {
            var facts = new[]
            {
                new ObservedFact("REVENUE", request.Revenue),
                new ObservedFact("EMPLOYEE_COUNT", request.EmployeeCount)
                
            };

            var rule = new MicroenterpriseEligibilityRule();
            var result = rule.Evaluate(facts);

            return Results.Ok(new
            {
                eligible = result.HasAssertion("MICROENTERPRISE_ELIGIBLE", true),
                explanation = result.Explanation?.Text
            });
        });
    }
}

public record MicroenterpriseRequest(
    decimal Revenue,
    int EmployeeCount
);