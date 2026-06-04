using System.Collections.Generic;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The outcome of resolving a structural legal address: a status, the candidate
/// matches (one when resolved, several when ambiguous, none when unresolved) and,
/// when unresolved, the originating query. Pure data; no resolution logic.
/// </summary>
public sealed record ResolutionResult(
    ResolutionStatus Status,
    IReadOnlyList<ResolutionCandidate> Candidates,
    UnresolvedReference? Unresolved)
{
    public bool IsResolved => Status == ResolutionStatus.Resolved;

    public bool IsAmbiguous => Status == ResolutionStatus.Ambiguous;

    public bool IsUnresolved => Status == ResolutionStatus.Unresolved;

    public FullyQualifiedLegalReference? ResolvedReference =>
        IsResolved && Candidates.Count == 1 ? Candidates[0].Reference : null;
}
