namespace FiscalOS.Core;

public sealed record TaxIdentificationNumber
{
    public string Value { get; }

    public TaxIdentificationNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Tax identification number cannot be empty.",
                nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}