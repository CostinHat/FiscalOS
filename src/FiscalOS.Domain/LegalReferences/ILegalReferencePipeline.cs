namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Orchestration contract for legal reference processing. An implementation runs
/// a sequence of <see cref="ILegalReferenceStage"/>s over a
/// <see cref="LegalReferenceContext"/> and returns the resulting context. No
/// orchestration runtime, persistence, or extraction logic is prescribed.
/// </summary>
public interface ILegalReferencePipeline
{
    Task<LegalReferenceContext> RunAsync(LegalReferenceContext context, CancellationToken cancellationToken = default);
}
