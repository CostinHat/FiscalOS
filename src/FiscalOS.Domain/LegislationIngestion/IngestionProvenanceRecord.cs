namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionProvenanceRecord
{
    public IngestionProvenanceId Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public IngestionBatchId BatchId { get; }

    public IngestionProvenanceCategory Category { get; }

    public LegislationSourceId? SourceId { get; }

    public RawDocumentId? RawDocumentId { get; }

    public SourceMetadataSnapshotId? SourceMetadataSnapshotId { get; }

    public ConfigurationSnapshotId? ConfigurationSnapshotId { get; }

    public IngestionCorrelationId? CorrelationId { get; }

    public IngestionCausationId? CausationId { get; }

    public string Description { get; }

    public IngestionProvenanceRecord(
        IngestionProvenanceId id,
        DateTimeOffset createdAt,
        IngestionBatchId batchId,
        IngestionProvenanceCategory category,
        LegislationSourceId? sourceId,
        RawDocumentId? rawDocumentId,
        SourceMetadataSnapshotId? sourceMetadataSnapshotId,
        ConfigurationSnapshotId? configurationSnapshotId,
        string description,
        IngestionCorrelationId? correlationId = null,
        IngestionCausationId? causationId = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(batchId);
        ArgumentNullException.ThrowIfNull(category);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Ingestion provenance description cannot be empty.", nameof(description));
        }

        Id = id;
        CreatedAt = createdAt;
        BatchId = batchId;
        Category = category;
        SourceId = sourceId;
        RawDocumentId = rawDocumentId;
        SourceMetadataSnapshotId = sourceMetadataSnapshotId;
        ConfigurationSnapshotId = configurationSnapshotId;
        CorrelationId = correlationId;
        CausationId = causationId;
        Description = description.Trim();
    }
}
