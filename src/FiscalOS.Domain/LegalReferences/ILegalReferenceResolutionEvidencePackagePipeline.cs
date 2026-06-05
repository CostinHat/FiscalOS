namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Orchestration contract for resolution evidence package processing. An
/// implementation runs a sequence of
/// <see cref="ILegalReferenceResolutionEvidencePackageStage"/>s over a
/// <see cref="LegalReferenceResolutionEvidencePackageContext"/> and returns the
/// resulting context. No engine, persistence, or resolution logic is prescribed.
/// </summary>
public interface ILegalReferenceResolutionEvidencePackagePipeline
{
    Task<LegalReferenceResolutionEvidencePackageContext> RunAsync(LegalReferenceResolutionEvidencePackageContext context, CancellationToken cancellationToken = default);
}
