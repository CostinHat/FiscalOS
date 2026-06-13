namespace FiscalOS.Domain.LegislationIngestion;

public sealed record ConfigurationSnapshotId
{
    public string Value { get; }

    public ConfigurationSnapshotId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Configuration snapshot id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
