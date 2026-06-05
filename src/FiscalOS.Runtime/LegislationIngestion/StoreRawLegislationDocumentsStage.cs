using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class StoreRawLegislationDocumentsStage : ILegislationIngestionStage
{
    private readonly IRawLegislationDocumentRepository _repository;
    private readonly Func<DateTimeOffset> _timestampProvider;

    public StoreRawLegislationDocumentsStage(IRawLegislationDocumentRepository repository)
        : this(repository, () => DateTimeOffset.UtcNow)
    {
    }

    public StoreRawLegislationDocumentsStage(
        IRawLegislationDocumentRepository repository,
        Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _repository = repository;
        _timestampProvider = timestampProvider;
    }

    public IngestionStage Stage => IngestionStage.CuratedPromotion;

    public async Task<IngestionContext> ExecuteAsync(
        IngestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var document in context.Documents)
        {
            await _repository.StoreAsync(document, cancellationToken);
        }

        var trace = context.Trace.Concat(new[]
        {
            new IngestionTraceEntry(
                Stage,
                IngestionStatus.Succeeded,
                _timestampProvider(),
                $"Stored {context.Documents.Count} raw legislation document(s)."),
        }).ToList();

        return context with { Trace = trace };
    }
}
