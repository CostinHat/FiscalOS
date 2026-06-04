namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Orchestration contract for resolution provenance processing. An implementation
/// runs a sequence of <see cref="ILegalReferenceResolutionProvenanceStage"/>s over
/// a <see cref="LegalReferenceResolutionProvenanceContext"/> and returns the
/// resulting context. No provenance engine, persistence, or resolution logic is
/// prescribed.
/// </summary>
public interface ILegalReferenceResolutionProvenancePipeline
{
    Task<LegalReferenceResolutionProvenanceContext> RunAsync(LegalReferenceResolutionProvenanceContext context, CancellationToken cancellationToken = default);
}
