using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class InMemoryLegalReferenceResolutionRepository : ILegalReferenceResolutionRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<LegalReference, ResolutionResult> _store = new();

    public Task StoreAsync(
        LegalReference query,
        ResolutionResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(result);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _store[query] = result;
        }

        return Task.CompletedTask;
    }

    public Task<ResolutionResult?> GetAsync(
        LegalReference query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(_store.TryGetValue(query, out var result) ? result : null);
        }
    }

    public Task<IReadOnlyList<ResolutionResult>> GetByStatusAsync(
        ResolutionStatus status,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<ResolutionResult>>(
                _store.Values.Where(result => result.Status == status).ToList());
        }
    }

    public Task<IReadOnlyList<UnresolvedReference>> GetUnresolvedAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<UnresolvedReference>>(
                _store.Values
                    .Where(result => result.Unresolved is not null)
                    .Select(result => result.Unresolved!)
                    .ToList());
        }
    }
}
