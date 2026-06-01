namespace FiscalOS.Runtime.Evaluation;

public sealed record RuleEvaluationResult(
    string RuleId,
    bool Passed,
    string? Message = null);