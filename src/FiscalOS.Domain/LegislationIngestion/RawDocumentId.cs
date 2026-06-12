namespace FiscalOS.Domain.LegislationIngestion;

public sealed record RawDocumentId
{
    public string Value { get; }

    public RawDocumentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Raw document id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
