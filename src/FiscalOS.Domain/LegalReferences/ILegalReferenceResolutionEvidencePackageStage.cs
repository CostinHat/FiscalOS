namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A single step in a resolution evidence package pipeline. A stage transforms a
/// <see cref="LegalReferenceResolutionEvidencePackageContext"/> and returns the
/// updated context. It is a pure contract; no engine, persistence, or resolution
/// logic is prescribed.
/// </summary>
public interface ILegalReferenceResolutionEvidencePackageStage
{
    string Name { get; }

    Task<LegalReferenceResolutionEvidencePackageContext> ExecuteAsync(LegalReferenceResolutionEvidencePackageContext context, CancellationToken cancellationToken = default);
}
