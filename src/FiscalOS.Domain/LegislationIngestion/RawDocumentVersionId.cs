namespace FiscalOS.Domain.LegislationIngestion;

public sealed record RawDocumentVersionId
{
    public string Value { get; }

    public RawDocumentVersionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Raw document version id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
