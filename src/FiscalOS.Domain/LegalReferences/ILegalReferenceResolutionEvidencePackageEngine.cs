namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The top-level contract for processing <see cref="ResolutionEvidencePackage"/>
/// records. It is a pure facade over evidence package pipeline processing; no
/// packaging algorithm, persistence, or interpretation is prescribed.
/// </summary>
public interface ILegalReferenceResolutionEvidencePackageEngine
{
    Task<IReadOnlyList<ResolutionEvidencePackage>> ProcessAsync(
        IReadOnlyList<ResolutionEvidencePackage> packages,
        CancellationToken cancellationToken = default);
}
