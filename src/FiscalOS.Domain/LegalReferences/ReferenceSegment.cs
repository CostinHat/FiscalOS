namespace FiscalOS.Domain.LegalReferences;

public sealed record ReferenceSegment
{
    public string Kind { get; }

    public string Value { get; }

    public ReferenceSegment(string kind, string value)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            throw new ArgumentException("Reference segment kind cannot be empty.", nameof(kind));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Reference segment value cannot be empty.", nameof(value));
        }

        Kind = kind.Trim();
        Value = value.Trim();
    }

    public override string ToString()
    {
        return $"{Kind} {Value}";
    }
}
