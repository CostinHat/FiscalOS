using System.Collections.Generic;

namespace FiscalOS.Domain.TerminologyReconciliation;

public sealed record ReconciliationResult(
    IReadOnlyList<TermMapping> Mappings,
    IReadOnlyList<SourceTerm> UnmappedSourceTerms)
{
    public bool IsFullyReconciled => UnmappedSourceTerms.Count == 0;
}
