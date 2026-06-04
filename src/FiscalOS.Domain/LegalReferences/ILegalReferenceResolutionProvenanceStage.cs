namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A single step in a resolution provenance pipeline. A stage transforms a
/// <see cref="LegalReferenceResolutionProvenanceContext"/> and returns the updated
/// context. It is a pure contract; no provenance engine, persistence, or
/// resolution logic is prescribed.
/// </summary>
public interface ILegalReferenceResolutionProvenanceStage
{
    string Name { get; }

    Task<LegalReferenceResolutionProvenanceContext> ExecuteAsync(LegalReferenceResolutionProvenanceContext context, CancellationToken cancellationToken = default);
}
