namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionBatchId
{
    public string Value { get; }

    public IngestionBatchId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Ingestion batch id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
