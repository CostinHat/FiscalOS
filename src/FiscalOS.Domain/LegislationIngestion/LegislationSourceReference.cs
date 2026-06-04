namespace FiscalOS.Domain.LegislationIngestion;

public sealed record LegislationSourceReference
{
    public string Value { get; }

    public LegislationSourceReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Legislation source reference cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
