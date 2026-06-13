namespace FiscalOS.Domain.LegislationIngestion;

public sealed record SourceMetadataSnapshot
{
    public SourceMetadataSnapshotId Id { get; }

    public LegislationSourceId SourceId { get; }

    public LegislationSourceMetadata Metadata { get; }

    public DateTimeOffset CreatedAt { get; }

    public string SchemaVersion { get; }

    public SourceMetadataSnapshot(
        SourceMetadataSnapshotId id,
        LegislationSourceMetadata metadata,
        DateTimeOffset createdAt,
        string schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(metadata);

        if (string.IsNullOrWhiteSpace(schemaVersion))
        {
            throw new ArgumentException("Source metadata snapshot schema version cannot be empty.", nameof(schemaVersion));
        }

        Id = id;
        SourceId = metadata.Id;
        Metadata = metadata;
        CreatedAt = createdAt;
        SchemaVersion = schemaVersion.Trim();
    }
}
