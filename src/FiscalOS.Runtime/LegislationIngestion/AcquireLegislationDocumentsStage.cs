using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class AcquireLegislationDocumentsStage : ILegislationIngestionStage
{
    private readonly ILegislationSource _source;
    private readonly Func<DateTimeOffset> _timestampProvider;

    public AcquireLegislationDocumentsStage(ILegislationSource source)
        : this(source, () => DateTimeOffset.UtcNow)
    {
    }

    public AcquireLegislationDocumentsStage(
        ILegislationSource source,
        Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _source = source;
        _timestampProvider = timestampProvider;
    }

    public IngestionStage Stage => IngestionStage.Acquisition;

    public async Task<IngestionContext> ExecuteAsync(
        IngestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var documents = await _source.FetchAsync(cancellationToken);
        var updatedDocuments = context.Documents.Concat(documents).ToList();
        var trace = context.Trace.Concat(new[]
        {
            new IngestionTraceEntry(
                Stage,
                IngestionStatus.Succeeded,
                _timestampProvider(),
                $"Acquired {documents.Count} document(s)."),
        }).ToList();

        return context with
        {
            Documents = updatedDocuments,
            Trace = trace,
        };
    }
}
