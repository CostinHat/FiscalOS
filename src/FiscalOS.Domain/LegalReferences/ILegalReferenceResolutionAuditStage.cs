namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A single step in a resolution audit pipeline. A stage transforms a
/// <see cref="LegalReferenceResolutionAuditContext"/> and returns the updated
/// context. It is a pure contract; no audit engine, persistence, or resolution
/// logic is prescribed.
/// </summary>
public interface ILegalReferenceResolutionAuditStage
{
    string Name { get; }

    Task<LegalReferenceResolutionAuditContext> ExecuteAsync(LegalReferenceResolutionAuditContext context, CancellationToken cancellationToken = default);
}
