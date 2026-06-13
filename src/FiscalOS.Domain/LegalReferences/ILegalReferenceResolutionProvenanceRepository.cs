using FiscalOS.LegalCore;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Persistence contract for legal reference resolution provenance. Implementations
/// store <see cref="ResolutionProvenance"/> against the structural
/// <see cref="LegalReference"/> query it describes, and answer query- and
/// source-based lookups. The contract prescribes no provenance engine, resolution
/// engine, or storage technology.
/// </summary>
public interface ILegalReferenceResolutionProvenanceRepository
{
    Task StoreAsync(ResolutionProvenance provenance, CancellationToken cancellationToken = default);

    Task<ResolutionProvenance?> GetAsync(LegalReference query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResolutionProvenance>> GetBySourceAsync(string source, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProvenanceChain>> GetChainsAsync(CancellationToken cancellationToken = default);
}
