using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class InMemoryRawLegislationDocumentRepository : IRawLegislationDocumentRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<LegislationDocumentId, RawLegislationDocument> _store = new();

    public Task StoreAsync(
        RawLegislationDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _store[document.Id] = document;
        }

        return Task.CompletedTask;
    }

    public Task<RawLegislationDocument?> GetAsync(
        LegislationDocumentId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(_store.TryGetValue(id, out var document) ? document : null);
        }
    }
}
