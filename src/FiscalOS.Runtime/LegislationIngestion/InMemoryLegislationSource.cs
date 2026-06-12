using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class InMemoryLegislationSource : ILegislationSource
{
    private static readonly LegislationSourceMetadata DefaultMetadata = new(
        new LegislationSourceId("in-memory-legislation-source"),
        "In-memory legislation source",
        "in-memory");

    private readonly object _sync = new();
    private readonly List<RawLegislationDocument> _documents;

    public InMemoryLegislationSource(params RawLegislationDocument[] documents)
        : this((IReadOnlyList<RawLegislationDocument>)documents)
    {
    }

    public InMemoryLegislationSource(IReadOnlyList<RawLegislationDocument> documents)
        : this(DefaultMetadata, documents)
    {
    }

    public InMemoryLegislationSource(
        LegislationSourceMetadata metadata,
        params RawLegislationDocument[] documents)
        : this(metadata, (IReadOnlyList<RawLegislationDocument>)documents)
    {
    }

    public InMemoryLegislationSource(
        LegislationSourceMetadata metadata,
        IReadOnlyList<RawLegislationDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(documents);

        Metadata = metadata;
        _documents = documents.ToList();
    }

    public LegislationSourceId Id => Metadata.Id;

    public LegislationSourceMetadata Metadata { get; }

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
