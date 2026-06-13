using FiscalOS.LegalCore;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A single audit record describing how a structural <see cref="LegalReference"/>
/// query was decided: the <see cref="ResolutionDecision"/> taken, the supporting
/// <see cref="ResolutionEvidence"/>, and when it occurred. Pure audit data.
/// </summary>
public sealed record ResolutionAuditEntry(
    LegalReference Query,
    ResolutionDecision Decision,
    ResolutionEvidence Evidence,
    DateTimeOffset Timestamp);
