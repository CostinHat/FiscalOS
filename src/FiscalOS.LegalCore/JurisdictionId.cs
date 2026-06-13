namespace FiscalOS.LegalCore;

public sealed record JurisdictionId
{
    public string Value { get; }

    public JurisdictionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Jurisdiction id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
