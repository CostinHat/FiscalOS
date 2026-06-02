
using FiscalOS.Core;

namespace FiscalOS.Runtime;

public sealed class MicroenterpriseEligibilityRule
{
    public EvaluationResult Evaluate(decimal revenue, int employeeCount)
    {
        var evidence = new[]
        {
            ObservedFact.Revenue(revenue),
            ObservedFact.EmployeeCount(employeeCount)
        };

        if (revenue <= MicroenterpriseRegime.RevenueThreshold
            && employeeCount >= MicroenterpriseRegime.MinimumEmployeeCount)
        {
            var assertion = new KnowledgeAssertion(
                "MICROENTERPRISE_ELIGIBLE",
                true,
                evidence);

            var explanation = new Explanation(
                $"MICROENTERPRISE_ELIGIBLE because Revenue={revenue} <= {MicroenterpriseRegime.RevenueThreshold} and EmployeeCount={employeeCount} >= {MicroenterpriseRegime.MinimumEmployeeCount}.",
                evidence);

            return new EvaluationResult([assertion], explanation);
        }

        return new EvaluationResult(
            [],
            new Explanation(
                "MICROENTERPRISE_ELIGIBLE was not produced because at least one condition failed.",
                evidence));
    }

    public EvaluationResult Evaluate(FiscalSubject subject)
    {
        return Evaluate(subject.Revenue, subject.EmployeeCount);
    }

    // Surfaces the governing citation when the regime threshold is satisfied,
    // so the citation originates from the layer that applies the threshold.
    public MicroenterpriseEvaluation EvaluateWithCitation(FiscalSubject subject)
    {
        var result = Evaluate(subject);

        var citation = result.HasAssertion("MICROENTERPRISE_ELIGIBLE", true)
            ? MicroenterpriseRegime.Citation
            : null;

        return new MicroenterpriseEvaluation(result, citation);
    }
}