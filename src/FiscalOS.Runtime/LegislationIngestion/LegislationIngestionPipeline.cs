using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class LegislationIngestionPipeline : ILegislationIngestionPipeline
{
    private readonly IReadOnlyList<ILegislationIngestionStage> _stages;
    private readonly Func<DateTimeOffset> _timestampProvider;
    private readonly IIngestionEmissionIdPolicy _emissionIdPolicy;

    public LegislationIngestionPipeline(params ILegislationIngestionStage[] stages)
        : this(stages, () => DateTimeOffset.UtcNow)
    {
    }

    public LegislationIngestionPipeline(
        IReadOnlyList<ILegislationIngestionStage> stages,
        Func<DateTimeOffset> timestampProvider)
        : this(stages, timestampProvider, new DefaultIngestionEmissionIdPolicy())
    {
    }

    public LegislationIngestionPipeline(
        IReadOnlyList<ILegislationIngestionStage> stages,
        Func<DateTimeOffset> timestampProvider,
        IIngestionEmissionIdPolicy emissionIdPolicy)
    {
        ArgumentNullException.ThrowIfNull(stages);
        ArgumentNullException.ThrowIfNull(timestampProvider);
        ArgumentNullException.ThrowIfNull(emissionIdPolicy);

        _stages = stages.ToArray();
        _timestampProvider = timestampProvider;
        _emissionIdPolicy = emissionIdPolicy;
    }

    public async Task<IngestionResult> RunAsync(
        IngestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var startedAt = _timestampProvider();
        var current = context with
        {
            Provenance = AppendProvenance(
                context.Provenance,
                context.BatchId,
                _emissionIdPolicy,
                IngestionProvenanceCategory.Batch,
                startedAt,
                "Ingestion batch started.",
                "batch-started"),
            AuditEvents = AppendAuditEvent(
                context.AuditEvents,
                context.BatchId,
                _emissionIdPolicy,
                IngestionAuditEventKind.BatchStarted,
                IngestionAuditEventOutcome.Completed,
                startedAt,
                "Ingestion batch started.",
                "batch-started"),
        };

        foreach (var stage in _stages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                current = await stage.ExecuteAsync(current, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var failedAt = _timestampProvider();
                var failedTrace = current.Trace.Concat(new[]
                {
                    new IngestionTraceEntry(
                        stage.Stage,
                        IngestionStatus.Failed,
                        failedAt,
                        ex.Message),
                }).ToList();
                var provenance = AppendProvenance(
                    current.Provenance,
                    current.BatchId,
                    _emissionIdPolicy,
                    IngestionProvenanceCategory.Batch,
                    failedAt,
                    ex.Message,
                    "batch-failed");
                var auditEvents = AppendAuditEvent(
                    current.AuditEvents,
                    current.BatchId,
                    _emissionIdPolicy,
                    IngestionAuditEventKind.FailureRecorded,
                    IngestionAuditEventOutcome.Failed,
                    failedAt,
                    ex.Message,
                    "batch-failed");

                return new IngestionResult(
                    current.BatchId,
                    IngestionStatus.Failed,
                    failedTrace,
                    current.RawDocumentIdentityDecisions,
                    provenance,
                    auditEvents);
            }
        }

        var completedAt = _timestampProvider();
        var completedProvenance = AppendProvenance(
            current.Provenance,
            current.BatchId,
            _emissionIdPolicy,
            IngestionProvenanceCategory.Batch,
            completedAt,
            "Ingestion batch completed.",
            "batch-completed");
        var completedAuditEvents = AppendAuditEvent(
            current.AuditEvents,
            current.BatchId,
            _emissionIdPolicy,
            IngestionAuditEventKind.BatchCompleted,
            IngestionAuditEventOutcome.Completed,
            completedAt,
            "Ingestion batch completed.",
            "batch-completed");

        return new IngestionResult(
            current.BatchId,
            IngestionStatus.Succeeded,
            current.Trace,
            current.RawDocumentIdentityDecisions,
            completedProvenance,
            completedAuditEvents);
    }

    private static IReadOnlyList<IngestionProvenanceRecord> AppendProvenance(
        IReadOnlyList<IngestionProvenanceRecord> provenance,
        IngestionBatchId batchId,
        IIngestionEmissionIdPolicy emissionIdPolicy,
        IngestionProvenanceCategory category,
        DateTimeOffset createdAt,
        string description,
        string suffix)
    {
        return provenance.Concat(new[]
        {
            new IngestionProvenanceRecord(
                emissionIdPolicy.CreateProvenanceId(batchId, suffix),
                createdAt,
                batchId,
                category,
                null,
                null,
                null,
                null,
                description),
        }).ToList();
    }

    private static IReadOnlyList<IngestionAuditEventRecord> AppendAuditEvent(
        IReadOnlyList<IngestionAuditEventRecord> auditEvents,
        IngestionBatchId batchId,
        IIngestionEmissionIdPolicy emissionIdPolicy,
        IngestionAuditEventKind kind,
        IngestionAuditEventOutcome outcome,
        DateTimeOffset createdAt,
        string details,
        string suffix)
    {
        return auditEvents.Concat(new[]
        {
            new IngestionAuditEventRecord(
                emissionIdPolicy.CreateAuditEventId(batchId, suffix),
                createdAt,
                batchId,
                kind,
                outcome,
                null,
                null,
                null,
                null,
                details),
        }).ToList();
    }
}
