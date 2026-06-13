namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionProvenanceRecord
{
    public IngestionProvenanceId Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public IngestionBatchId BatchId { get; }

    public LegislationSourceId? SourceId { get; }

    public RawDocumentId? RawDocumentId { get; }

    public SourceMetadataSnapshotId? SourceMetadataSnapshotId { get; }

    public ConfigurationSnapshotId? ConfigurationSnapshotId { get; }

    public string Description { get; }

    public IngestionProvenanceRecord(
        IngestionProvenanceId id,
        DateTimeOffset createdAt,
        IngestionBatchId batchId,
        LegislationSourceId? sourceId,
        RawDocumentId? rawDocumentId,
        SourceMetadataSnapshotId? sourceMetadataSnapshotId,
        ConfigurationSnapshotId? configurationSnapshotId,
        string description)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(batchId);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Ingestion provenance description cannot be empty.", nameof(description));
        }

        Id = id;
        CreatedAt = createdAt;
        BatchId = batchId;
        SourceId = sourceId;
        RawDocumentId = rawDocumentId;
        SourceMetadataSnapshotId = sourceMetadataSnapshotId;
        ConfigurationSnapshotId = configurationSnapshotId;
        Description = description.Trim();
    }
}
