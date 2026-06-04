namespace FiscalOS.Domain.LegalReferences;

public sealed record LegalDocumentReference
{
    public string Value { get; }

    public LegalDocumentReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Legal document reference cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
