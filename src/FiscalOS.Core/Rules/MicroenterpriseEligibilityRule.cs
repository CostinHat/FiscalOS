namespace FiscalOS.Core.Rules;
public sealed class MicroenterpriseEligibilityRule : IRule
{
    public EvaluationResult Evaluate(IEnumerable<ObservedFact> facts)
    {
        var revenue = facts.FirstOrDefault(x => x.ConceptCode == "REVENUE");
        var employeeCount = facts.FirstOrDefault(x => x.ConceptCode == "EMPLOYEE_COUNT");

        if (revenue is null || employeeCount is null)
        {
            return new EvaluationResult(
                Array.Empty<KnowledgeAssertion>(),
                new Explanation("Missing required facts.", facts.ToArray()));
        }

        var revenueValue = Convert.ToDecimal(revenue.Value);
        var employeeCountValue = Convert.ToInt32(employeeCount.Value);

        if (revenueValue <= 500000m && employeeCountValue >= 1)
        {
            return new EvaluationResult(
                new[]
                {
                    new KnowledgeAssertion("MICROENTERPRISE_ELIGIBLE", true, facts.ToArray())
                },
                new Explanation(
                    $"Revenue={revenueValue} and EmployeeCount={employeeCountValue}",
                    facts.ToArray()));
        }

        return new EvaluationResult(
            Array.Empty<KnowledgeAssertion>(),
            new Explanation("Microenterprise eligibility conditions not met.", facts.ToArray()));
    }
}