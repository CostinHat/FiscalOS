using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class AcquireLegislationDocumentsStage : ILegislationIngestionStage
{
    private readonly ILegislationSource _source;
    private readonly Func<DateTimeOffset> _timestampProvider;
    private readonly IIngestionEmissionIdPolicy _emissionIdPolicy;

    public AcquireLegislationDocumentsStage(ILegislationSource source)
        : this(source, () => DateTimeOffset.UtcNow)
    {
    }

    public AcquireLegislationDocumentsStage(
        ILegislationSource source,
        Func<DateTimeOffset> timestampProvider)
        : this(source, timestampProvider, new DefaultIngestionEmissionIdPolicy())
    {
    }

    public AcquireLegislationDocumentsStage(
        ILegislationSource source,
        Func<DateTimeOffset> timestampProvider,
        IIngestionEmissionIdPolicy emissionIdPolicy)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(timestampProvider);
        ArgumentNullException.ThrowIfNull(emissionIdPolicy);

        _source = source;
        _timestampProvider = timestampProvider;
        _emissionIdPolicy = emissionIdPolicy;
    }

    public IngestionStage Stage => IngestionStage.Acquisition;

    public async Task<IngestionContext> ExecuteAsync(
        IngestionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var sourceSelectedAt = _timestampProvider();
        var sourceAcquisitionStartedAt = _timestampProvider();
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
        var provenance = AppendSourceProvenance(
            context.Provenance,
            context.BatchId,
            _source.Id,
            sourceSelectedAt);
        provenance = AppendCandidateProvenance(
            provenance,
            context.BatchId,
            _source.Id,
            documents);
        var auditEvents = AppendSourceAuditEvents(
            context.AuditEvents,
            context.BatchId,
            _source.Id,
            sourceSelectedAt,
            sourceAcquisitionStartedAt);
        auditEvents = AppendCandidateAuditEvents(
            auditEvents,
            context.BatchId,
            _source.Id,
            documents);

        return context with
        {
            Documents = updatedDocuments,
            Trace = trace,
            Provenance = provenance,
            AuditEvents = auditEvents,
        };
    }

    private IReadOnlyList<IngestionProvenanceRecord> AppendSourceProvenance(
        IReadOnlyList<IngestionProvenanceRecord> provenance,
        IngestionBatchId batchId,
        LegislationSourceId sourceId,
        DateTimeOffset createdAt)
    {
        return provenance.Concat(new[]
        {
            new IngestionProvenanceRecord(
                _emissionIdPolicy.CreateProvenanceId(
                    batchId,
                    $"source-selected:{sourceId.Value}"),
                createdAt,
                batchId,
                IngestionProvenanceCategory.Source,
                sourceId,
                null,
                null,
                null,
                "Source selected."),
        }).ToList();
    }

    private IReadOnlyList<IngestionAuditEventRecord> AppendSourceAuditEvents(
        IReadOnlyList<IngestionAuditEventRecord> auditEvents,
        IngestionBatchId batchId,
        LegislationSourceId sourceId,
        DateTimeOffset sourceSelectedAt,
        DateTimeOffset sourceAcquisitionStartedAt)
    {
        return auditEvents.Concat(new[]
        {
            new IngestionAuditEventRecord(
                _emissionIdPolicy.CreateAuditEventId(
                    batchId,
                    $"source-selected:{sourceId.Value}"),
                sourceSelectedAt,
                batchId,
                IngestionAuditEventKind.SourceSelected,
                IngestionAuditEventOutcome.Completed,
                sourceId,
                null,
                null,
                null,
                "Source selected."),
            new IngestionAuditEventRecord(
                _emissionIdPolicy.CreateAuditEventId(
                    batchId,
                    $"source-acquisition-started:{sourceId.Value}"),
                sourceAcquisitionStartedAt,
                batchId,
                IngestionAuditEventKind.SourceAcquisitionStarted,
                IngestionAuditEventOutcome.Completed,
                sourceId,
                null,
                null,
                null,
                "Source acquisition started."),
        }).ToList();
    }

    private IReadOnlyList<IngestionProvenanceRecord> AppendCandidateProvenance(
        IReadOnlyList<IngestionProvenanceRecord> provenance,
        IngestionBatchId batchId,
        LegislationSourceId sourceId,
        IReadOnlyList<RawLegislationDocument> documents)
    {
        var records = documents.Select(document =>
            new IngestionProvenanceRecord(
                _emissionIdPolicy.CreateProvenanceId(
                    batchId,
                    $"candidate-fetched:{document.Id.Value}"),
                _timestampProvider(),
                batchId,
                IngestionProvenanceCategory.RawDocument,
                sourceId,
                null,
                null,
                null,
                $"Candidate fetched: {document.Id.Value}."));

        return provenance.Concat(records).ToList();
    }

    private IReadOnlyList<IngestionAuditEventRecord> AppendCandidateAuditEvents(
        IReadOnlyList<IngestionAuditEventRecord> auditEvents,
        IngestionBatchId batchId,
        LegislationSourceId sourceId,
        IReadOnlyList<RawLegislationDocument> documents)
    {
        var records = documents.Select(document =>
            new IngestionAuditEventRecord(
                _emissionIdPolicy.CreateAuditEventId(
                    batchId,
                    $"candidate-fetched:{document.Id.Value}"),
                _timestampProvider(),
                batchId,
                IngestionAuditEventKind.CandidateFetched,
                IngestionAuditEventOutcome.Completed,
                sourceId,
                null,
                null,
                null,
                $"Candidate fetched: {document.Id.Value}."));

        return auditEvents.Concat(records).ToList();
    }
}
