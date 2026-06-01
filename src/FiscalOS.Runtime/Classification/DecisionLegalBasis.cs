using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public sealed record DecisionLegalBasis(
    IReadOnlyList<LegalCitation> ConsideredCitations,
    ConflictResolutionResult Resolution)
{
    public IReadOnlyList<LegalCitation> GoverningCitations => Resolution.Winners;

    public bool IsResolved => Resolution.IsResolved;

    public bool IsUnresolved => Resolution.IsUnresolved;

    public bool IsEmpty => Resolution.IsEmpty;

    public static DecisionLegalBasis Resolve(IReadOnlyList<LegalCitation> consideredCitations)
    {
        ArgumentNullException.ThrowIfNull(consideredCitations);

        return new DecisionLegalBasis(
            consideredCitations,
            ConflictResolver.Resolve(consideredCitations));
    }
}
