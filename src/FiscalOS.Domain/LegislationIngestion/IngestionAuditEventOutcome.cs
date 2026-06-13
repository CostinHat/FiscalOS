namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionAuditEventOutcome
{
    public static readonly IngestionAuditEventOutcome Completed = new("completed");
    public static readonly IngestionAuditEventOutcome Skipped = new("skipped");
    public static readonly IngestionAuditEventOutcome Failed = new("failed");
    public static readonly IngestionAuditEventOutcome Deferred = new("deferred");

    public string Value { get; }

    public IngestionAuditEventOutcome(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Ingestion audit event outcome cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
