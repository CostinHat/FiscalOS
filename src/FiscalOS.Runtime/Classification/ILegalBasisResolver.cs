using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public interface ILegalBasisResolver
{
    DecisionLegalBasis Resolve(IReadOnlyList<LegalCitation> consideredCitations);
}
