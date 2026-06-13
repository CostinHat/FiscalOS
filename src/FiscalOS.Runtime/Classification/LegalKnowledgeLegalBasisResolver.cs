using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public sealed class LegalKnowledgeLegalBasisResolver : ILegalBasisResolver
{
    public DecisionLegalBasis Resolve(IReadOnlyList<LegalCitation> consideredCitations)
    {
        ArgumentNullException.ThrowIfNull(consideredCitations);

        return new DecisionLegalBasis(
            consideredCitations,
            ConflictResolver.Resolve(consideredCitations));
    }
}
