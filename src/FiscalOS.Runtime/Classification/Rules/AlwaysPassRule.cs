using FiscalOS.Runtime.Evaluation;

namespace FiscalOS.Runtime.Classification.Rules;

public sealed class AlwaysPassRule : ClassificationRule
{
    public string RuleId => "ALWAYS_PASS";

    public string Description =>
        "Temporary rule used for pipeline validation.";

        public int Priority => 0;

    public RuleEvaluationResult Evaluate(
        ClassificationContext context)
    {
        return new RuleEvaluationResult(
            RuleId,
            true,
            "Rule executed successfully.");
    }
}