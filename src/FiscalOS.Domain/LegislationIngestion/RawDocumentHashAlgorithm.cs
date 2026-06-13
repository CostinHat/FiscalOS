namespace FiscalOS.Domain.LegislationIngestion;

public sealed record RawDocumentHashAlgorithm
{
    public string Value { get; }

    public RawDocumentHashAlgorithm(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Raw document hash algorithm cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
