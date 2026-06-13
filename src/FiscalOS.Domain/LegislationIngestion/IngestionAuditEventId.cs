namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionAuditEventId
{
    public string Value { get; }

    public IngestionAuditEventId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Ingestion audit event id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
