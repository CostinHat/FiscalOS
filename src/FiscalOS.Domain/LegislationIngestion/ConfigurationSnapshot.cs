namespace FiscalOS.Domain.LegislationIngestion;

public sealed record ConfigurationSnapshot
{
    public ConfigurationSnapshotId Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public string SchemaVersion { get; }

    public ConfigurationSnapshot(
        ConfigurationSnapshotId id,
        DateTimeOffset createdAt,
        string schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(schemaVersion))
        {
            throw new ArgumentException("Configuration snapshot schema version cannot be empty.", nameof(schemaVersion));
        }

        Id = id;
        CreatedAt = createdAt;
        SchemaVersion = schemaVersion.Trim();
    }
}
