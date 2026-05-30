using FiscalOS.Core;

namespace FiscalOS.Runtime;

public sealed class MicroenterpriseEligibilityRule
{
    private const decimal RevenueThreshold = 500_000m;

    public EvaluationResult Evaluate(decimal revenue, int employeeCount)
    {
        var evidence = new[]
        {
            ObservedFact.Revenue(revenue),
            ObservedFact.EmployeeCount(employeeCount)
        };

        if (revenue <= RevenueThreshold && employeeCount >= 1)
        {
            var assertion = new KnowledgeAssertion(
                "MICROENTERPRISE_ELIGIBLE",
                true,
                evidence);

            var explanation = new Explanation(
                $"MICROENTERPRISE_ELIGIBLE because Revenue={revenue} <= {RevenueThreshold} and EmployeeCount={employeeCount} >= 1.",
                evidence);

            return new EvaluationResult([assertion], explanation);
        }

        return new EvaluationResult([], new Explanation(
            "MICROENTERPRISE_ELIGIBLE was not produced because at least one condition failed.",
            evidence));
    }
}
