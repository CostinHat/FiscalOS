namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionProvenanceCategory
{
    public static readonly IngestionProvenanceCategory Source = new("source");
    public static readonly IngestionProvenanceCategory Batch = new("batch");
    public static readonly IngestionProvenanceCategory Snapshot = new("snapshot");
    public static readonly IngestionProvenanceCategory RawDocument = new("raw-document");
    public static readonly IngestionProvenanceCategory Identity = new("identity");
    public static readonly IngestionProvenanceCategory Configuration = new("configuration");

    public string Value { get; }

    public IngestionProvenanceCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Ingestion provenance category cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
