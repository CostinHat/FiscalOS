namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The top-level contract for producing a <see cref="ResolutionAuditTrail"/> from
/// a set of <see cref="ResolutionResult"/> outcomes. It is a pure facade; no audit
/// algorithm, persistence, or interpretation is prescribed.
/// </summary>
public interface ILegalReferenceResolutionAuditEngine
{
    Task<ResolutionAuditTrail> AuditAsync(
        IReadOnlyList<ResolutionResult> results,
        CancellationToken cancellationToken = default);
}
