namespace FiscalOS.Domain.LegislationIngestion;

public sealed record LegislationSourceId
{
    public string Value { get; }

    public LegislationSourceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Legislation source id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
