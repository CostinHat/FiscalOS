namespace FiscalOS.Domain.LegislationIngestion;

public sealed record RawDocumentContentHash
{
    public string Value { get; }

    public RawDocumentContentHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Raw document content hash cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
