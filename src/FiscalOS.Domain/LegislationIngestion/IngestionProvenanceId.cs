namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionProvenanceId
{
    public string Value { get; }

    public IngestionProvenanceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Ingestion provenance id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
