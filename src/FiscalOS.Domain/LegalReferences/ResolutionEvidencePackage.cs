namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// An immutable evidence package composing the three independent concerns of a
/// single legal reference resolution: the <see cref="ResolutionResult"/> outcome,
/// its <see cref="ResolutionAuditTrail"/>, and its <see cref="ResolutionProvenance"/>.
/// It composes them without merging them — each concern remains separate and is
/// exposed as-is. Pure data; it carries no resolution, audit, or provenance logic.
/// </summary>
public sealed record ResolutionEvidencePackage(
    ResolutionResult Result,
    ResolutionAuditTrail AuditTrail,
    ResolutionProvenance Provenance);
