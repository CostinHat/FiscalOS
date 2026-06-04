namespace FiscalOS.Domain.TerminologyReconciliation;

public sealed record CanonicalTerm
{
    public string Value { get; }

    public CanonicalTerm(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Canonical term cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
