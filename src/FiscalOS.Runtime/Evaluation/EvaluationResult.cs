using System.Linq;

namespace FiscalOS.Runtime.Evaluation;

public sealed record EvaluationResult(
    IReadOnlyCollection<RuleEvaluationResult> Results)
{
    public bool Passed => Results.All(r => r.Passed);
}