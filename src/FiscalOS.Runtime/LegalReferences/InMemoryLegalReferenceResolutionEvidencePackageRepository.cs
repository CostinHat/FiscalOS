using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class InMemoryLegalReferenceResolutionEvidencePackageRepository : ILegalReferenceResolutionEvidencePackageRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<LegalReference, ResolutionEvidencePackage> _store = new();

    public Task StoreAsync(
        LegalReference query,
        ResolutionEvidencePackage package,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(package);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _store[query] = package;
        }

        return Task.CompletedTask;
    }

    public Task<ResolutionEvidencePackage?> GetAsync(
        LegalReference query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(_store.TryGetValue(query, out var package) ? package : null);
        }
    }
}
