namespace FiscalOS.Core.Classification;

public sealed record RuleEvaluationResult(
    string RuleId,
    bool Matched,
    string Explanation);