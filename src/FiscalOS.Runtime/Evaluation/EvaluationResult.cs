namespace FiscalOS.Runtime.Evaluation;

public sealed record EvaluationResult(
    bool IsCompliant,
    IReadOnlyCollection<RuleEvaluationResult> RuleResults);