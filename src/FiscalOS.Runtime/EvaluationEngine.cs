using FiscalOS.Core;

namespace FiscalOS.Runtime;

public static class EvaluationEngine
{
    public static EvaluationResult Evaluate(decimal revenue, int employeeCount)
    {
        var rule = new MicroenterpriseEligibilityRule();
        return rule.Evaluate(revenue, employeeCount);
    }
}
