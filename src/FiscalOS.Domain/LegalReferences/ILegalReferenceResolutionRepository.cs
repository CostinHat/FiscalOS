namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Persistence contract for legal reference resolution outcomes. Implementations
/// store a <see cref="ResolutionResult"/> against the structural
/// <see cref="LegalReference"/> query that produced it, and answer status-based
/// lookups. The contract prescribes no resolver engine, search algorithm, or
/// storage technology.
/// </summary>
public interface ILegalReferenceResolutionRepository
{
    Task StoreAsync(LegalReference query, ResolutionResult result, CancellationToken cancellationToken = default);

    Task<ResolutionResult?> GetAsync(LegalReference query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResolutionResult>> GetByStatusAsync(ResolutionStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UnresolvedReference>> GetUnresolvedAsync(CancellationToken cancellationToken = default);
}
