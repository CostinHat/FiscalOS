namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Persistence contract for structural legal references. Implementations store
/// <see cref="FullyQualifiedLegalReference"/> addresses and answer purely structural
/// hierarchy queries; the contract carries no legal content, interpretation, or
/// fiscal conclusions, and prescribes no storage technology.
/// </summary>
public interface ILegalReferenceRepository
{
    Task StoreAsync(FullyQualifiedLegalReference reference, CancellationToken cancellationToken = default);

    Task<FullyQualifiedLegalReference?> GetAsync(FullyQualifiedLegalReference reference, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FullyQualifiedLegalReference>> GetChildrenAsync(FullyQualifiedLegalReference parent, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FullyQualifiedLegalReference>> GetDescendantsAsync(FullyQualifiedLegalReference ancestor, CancellationToken cancellationToken = default);
}
