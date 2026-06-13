namespace FiscalOS.Domain.LegislationIngestion;

public sealed record SourceMetadataSnapshotId
{
    public string Value { get; }

    public SourceMetadataSnapshotId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Source metadata snapshot id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
