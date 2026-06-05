using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class LegislationIngestionPipeline : ILegislationIngestionPipeline
{
    private readonly IReadOnlyList<ILegislationIngestionStage> _stages;
    private readonly Func<DateTimeOffset> _timestampProvider;

    public LegislationIngestionPipeline(params ILegislationIngestionStage[] stages)
        : this(stages, () => DateTimeOffset.UtcNow)
    {
    }

    public LegislationIngestionPipeline(
        IReadOnlyList<ILegislationIngestionStage> stages,
        Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(stages);
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _stages = stages.ToArray();
        _timestampProvider = timestampProvider;
    }

    public async Task<IngestionResult> RunAsync(
        IngestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var current = context;
        foreach (var stage in _stages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                current = await stage.ExecuteAsync(current, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var failedTrace = current.Trace.Concat(new[]
                {
                    new IngestionTraceEntry(
                        stage.Stage,
                        IngestionStatus.Failed,
                        _timestampProvider(),
                        ex.Message),
                }).ToList();

                return new IngestionResult(
                    current.BatchId,
                    IngestionStatus.Failed,
                    failedTrace);
            }
        }

        return new IngestionResult(
            current.BatchId,
            IngestionStatus.Succeeded,
            current.Trace);
    }
}
