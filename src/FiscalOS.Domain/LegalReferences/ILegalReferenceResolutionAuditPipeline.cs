namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Orchestration contract for resolution audit processing. An implementation runs
/// a sequence of <see cref="ILegalReferenceResolutionAuditStage"/>s over a
/// <see cref="LegalReferenceResolutionAuditContext"/> and returns the resulting
/// context. No audit engine, persistence, or resolution logic is prescribed.
/// </summary>
public interface ILegalReferenceResolutionAuditPipeline
{
    Task<LegalReferenceResolutionAuditContext> RunAsync(LegalReferenceResolutionAuditContext context, CancellationToken cancellationToken = default);
}
