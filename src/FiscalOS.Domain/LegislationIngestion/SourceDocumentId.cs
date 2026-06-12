namespace FiscalOS.Domain.LegislationIngestion;

public sealed record SourceDocumentId
{
    public string Value { get; }

    public SourceDocumentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Source document id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
