using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class DiscoverLegislationDocumentsStage : ILegislationIngestionStage
{
    private readonly Func<DateTimeOffset> _timestampProvider;

    public DiscoverLegislationDocumentsStage()
        : this(() => DateTimeOffset.UtcNow)
    {
    }

    public DiscoverLegislationDocumentsStage(Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _timestampProvider = timestampProvider;
    }

    public IngestionStage Stage => IngestionStage.Discovery;

    public Task<IngestionContext> ExecuteAsync(
        IngestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var trace = context.Trace.Concat(new[]
        {
            new IngestionTraceEntry(
                Stage,
                IngestionStatus.Succeeded,
                _timestampProvider(),
                "Discovered legislation ingestion batch."),
        }).ToList();

        return Task.FromResult(context with { Trace = trace });
    }
}
