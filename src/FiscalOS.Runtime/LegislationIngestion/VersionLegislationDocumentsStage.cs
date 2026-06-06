using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class VersionLegislationDocumentsStage : ILegislationIngestionStage
{
    private readonly Func<DateTimeOffset> _timestampProvider;

    public VersionLegislationDocumentsStage()
        : this(() => DateTimeOffset.UtcNow)
    {
    }

    public VersionLegislationDocumentsStage(Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _timestampProvider = timestampProvider;
    }

    public IngestionStage Stage => IngestionStage.Versioning;

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
                $"Versioned {context.Documents.Count} raw legislation document(s)."),
        }).ToList();

        return Task.FromResult(context with
        {
            Documents = context.Documents.ToList(),
            Trace = trace,
        });
    }
}
