using FiscalOS.Core.Classification;

namespace FiscalOS.Runtime.Classification;

public sealed record ClassificationDecision(
    ClassificationResult Result,
    string? WinningRuleId,
    IReadOnlyList<Evaluation.RuleEvaluationResult> RuleResults);
