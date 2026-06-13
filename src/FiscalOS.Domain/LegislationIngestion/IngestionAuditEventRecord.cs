namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionAuditEventRecord
{
    public IngestionAuditEventId Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public IngestionBatchId BatchId { get; }

    public IngestionAuditEventKind Kind { get; }

    public IngestionAuditEventOutcome Outcome { get; }

    public LegislationSourceId? SourceId { get; }

    public RawDocumentId? RawDocumentId { get; }

    public SourceMetadataSnapshotId? SourceMetadataSnapshotId { get; }

    public ConfigurationSnapshotId? ConfigurationSnapshotId { get; }

    public string Details { get; }

    public IngestionAuditEventRecord(
        IngestionAuditEventId id,
        DateTimeOffset createdAt,
        IngestionBatchId batchId,
        IngestionAuditEventKind kind,
        IngestionAuditEventOutcome outcome,
        LegislationSourceId? sourceId,
        RawDocumentId? rawDocumentId,
        SourceMetadataSnapshotId? sourceMetadataSnapshotId,
        ConfigurationSnapshotId? configurationSnapshotId,
        string details)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(batchId);
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentNullException.ThrowIfNull(outcome);

        if (string.IsNullOrWhiteSpace(details))
        {
            throw new ArgumentException("Ingestion audit event details cannot be empty.", nameof(details));
        }

        Id = id;
        CreatedAt = createdAt;
        BatchId = batchId;
        Kind = kind;
        Outcome = outcome;
        SourceId = sourceId;
        RawDocumentId = rawDocumentId;
        SourceMetadataSnapshotId = sourceMetadataSnapshotId;
        ConfigurationSnapshotId = configurationSnapshotId;
        Details = details.Trim();
    }
}
