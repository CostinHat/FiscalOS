using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Evaluation;

public sealed record RuleEvaluationResult(
    string RuleId,
    bool Passed,
    string? Message = null,
    string? Category = null)
{
    public IReadOnlyList<LegalCitation> Citations { get; init; } = [];
}
