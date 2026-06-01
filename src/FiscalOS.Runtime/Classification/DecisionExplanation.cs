using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public sealed record DecisionExplanation(
    DecisionLegalBasis LegalBasis,
    AuditGraph AuditGraph)
{
    public bool HasUnresolvedLegalConflict => LegalBasis.IsUnresolved;

    public bool HasLegalBasis => !LegalBasis.IsEmpty;
}
