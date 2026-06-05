using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class LegislationIngestionRuntime
{
    private readonly ILegislationIngestionPipeline _pipeline;

    public LegislationIngestionRuntime(ILegislationIngestionPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        _pipeline = pipeline;
    }

    public static LegislationIngestionRuntime Create(
        ILegislationSource source,
        IRawLegislationDocumentRepository repository)
    {
        return Create(source, repository, () => DateTimeOffset.UtcNow);
    }

    public static LegislationIngestionRuntime Create(
        ILegislationSource source,
        IRawLegislationDocumentRepository repository,
        Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(timestampProvider);

        var pipeline = new LegislationIngestionPipeline(
            new ILegislationIngestionStage[]
            {
                new AcquireLegislationDocumentsStage(source, timestampProvider),
                new StoreRawLegislationDocumentsStage(repository, timestampProvider),
            },
            timestampProvider);

        return new LegislationIngestionRuntime(pipeline);
    }

    public Task<IngestionResult> RunAsync(
        IngestionBatchId batchId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batchId);

        var context = new IngestionContext(
            batchId,
            Array.Empty<RawLegislationDocument>(),
            Array.Empty<IngestionTraceEntry>());

        return _pipeline.RunAsync(context, cancellationToken);
    }
}
