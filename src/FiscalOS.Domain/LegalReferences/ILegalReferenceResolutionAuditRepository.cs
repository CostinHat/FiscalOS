namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Persistence contract for legal reference resolution audit data. Implementations
/// store <see cref="ResolutionAuditTrail"/>s and answer query- and decision-based
/// lookups over the recorded <see cref="ResolutionAuditEntry"/> records. The
/// contract prescribes no audit engine, resolution engine, or storage technology.
/// </summary>
public interface ILegalReferenceResolutionAuditRepository
{
    Task StoreAsync(ResolutionAuditTrail trail, CancellationToken cancellationToken = default);

    Task<ResolutionAuditTrail?> GetAsync(LegalReference query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResolutionAuditEntry>> GetByDecisionStatusAsync(ResolutionStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResolutionAuditEntry>> GetUnresolvedAsync(CancellationToken cancellationToken = default);
}
