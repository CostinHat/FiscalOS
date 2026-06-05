using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class InMemoryLegislationSource : ILegislationSource
{
    private readonly object _sync = new();
    private readonly List<RawLegislationDocument> _documents;

    public InMemoryLegislationSource(params RawLegislationDocument[] documents)
        : this((IReadOnlyList<RawLegislationDocument>)documents)
    {
    }

    public InMemoryLegislationSource(IReadOnlyList<RawLegislationDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        _documents = documents.ToList();
    }

    public Task<IReadOnlyList<RawLegislationDocument>> FetchAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<RawLegislationDocument>>(_documents.ToList());
        }
    }
}
