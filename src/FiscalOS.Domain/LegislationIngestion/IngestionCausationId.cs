namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionCausationId
{
    public string Value { get; }

    public IngestionCausationId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Ingestion causation id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
