namespace FiscalOS.Domain.TerminologyReconciliation;

public sealed record SourceTerm
{
    public string Value { get; }

    public SourceTerm(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Source term cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
