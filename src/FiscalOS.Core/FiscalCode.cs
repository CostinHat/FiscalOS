namespace FiscalOS.Core;

public sealed record FiscalCode
{
    public string Value { get; }

    public FiscalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Fiscal code cannot be empty.");
        }

        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}