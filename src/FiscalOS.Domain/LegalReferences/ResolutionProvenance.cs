namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The provenance of a resolution: the structural <see cref="LegalReference"/>
/// query, its <see cref="ResolutionStatus"/> outcome, the <see cref="ProvenanceChain"/>
/// of derivation steps, and the associated <see cref="ResolutionAuditTrail"/>.
/// Pure provenance data; no resolution logic.
/// </summary>
public sealed record ResolutionProvenance(
    LegalReference Query,
    ResolutionStatus Status,
    ProvenanceChain Chain,
    ResolutionAuditTrail AuditTrail);
