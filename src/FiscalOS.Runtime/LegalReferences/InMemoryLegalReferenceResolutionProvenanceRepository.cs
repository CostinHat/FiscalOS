using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class InMemoryLegalReferenceResolutionProvenanceRepository : ILegalReferenceResolutionProvenanceRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<LegalReference, ResolutionProvenance> _store = new();

    public Task StoreAsync(
        ResolutionProvenance provenance,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provenance);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _store[provenance.Query] = provenance;
        }

        return Task.CompletedTask;
    }

    public Task<ResolutionProvenance?> GetAsync(
        LegalReference query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(_store.TryGetValue(query, out var provenance) ? provenance : null);
        }
    }

    public Task<IReadOnlyList<ResolutionProvenance>> GetBySourceAsync(
        string source,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Provenance source cannot be empty.", nameof(source));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var normalized = source.Trim();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<ResolutionProvenance>>(
                _store.Values
                    .Where(provenance => provenance.Chain.Steps.Any(step => step.Source.Value == normalized))
                    .ToList());
        }
    }

    public Task<IReadOnlyList<ProvenanceChain>> GetChainsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<ProvenanceChain>>(
                _store.Values.Select(provenance => provenance.Chain).ToList());
        }
    }
}
