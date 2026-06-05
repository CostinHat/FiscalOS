namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Persistence contract for composed resolution evidence packages. Implementations
/// store a <see cref="ResolutionEvidencePackage"/> against the structural
/// <see cref="LegalReference"/> query it pertains to, and retrieve it by that key.
/// The contract prescribes no storage technology; it composes — and does not merge —
/// the resolution, audit, and provenance concerns the package carries.
/// </summary>
public interface ILegalReferenceResolutionEvidencePackageRepository
{
    Task StoreAsync(LegalReference query, ResolutionEvidencePackage package, CancellationToken cancellationToken = default);

    Task<ResolutionEvidencePackage?> GetAsync(LegalReference query, CancellationToken cancellationToken = default);
}
