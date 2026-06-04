namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Orchestration contract for legal reference resolution. An implementation runs
/// a sequence of <see cref="ILegalReferenceResolutionStage"/>s over a
/// <see cref="LegalReferenceResolutionContext"/> and returns the resulting
/// context. No resolution engine, persistence, or search logic is prescribed.
/// </summary>
public interface ILegalReferenceResolutionPipeline
{
    Task<LegalReferenceResolutionContext> RunAsync(LegalReferenceResolutionContext context, CancellationToken cancellationToken = default);
}
