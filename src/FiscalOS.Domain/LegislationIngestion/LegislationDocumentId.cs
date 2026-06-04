namespace FiscalOS.Domain.LegislationIngestion;

public sealed record LegislationDocumentId
{
    public string Value { get; }

    public LegislationDocumentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Legislation document id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
