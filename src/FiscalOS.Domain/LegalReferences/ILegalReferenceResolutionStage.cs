namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A single step in a legal reference resolution pipeline. A stage transforms a
/// <see cref="LegalReferenceResolutionContext"/> and returns the updated context.
/// It is a pure contract; no resolution engine, search algorithm, or persistence
/// is prescribed.
/// </summary>
public interface ILegalReferenceResolutionStage
{
    string Name { get; }

    Task<LegalReferenceResolutionContext> ExecuteAsync(LegalReferenceResolutionContext context, CancellationToken cancellationToken = default);
}
