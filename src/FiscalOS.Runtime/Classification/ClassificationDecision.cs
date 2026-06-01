using FiscalOS.Core.Classification;
using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public sealed record ClassificationDecision(
    ClassificationResult Result,
    string? WinningRuleId,
    IReadOnlyList<Evaluation.RuleEvaluationResult> RuleResults,
    DecisionLegalBasis LegalBasis,
    AuditGraph AuditGraph)
{
    public bool HasUnresolvedLegalConflict => LegalBasis.IsUnresolved;
}
