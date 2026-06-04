namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// Identifies the origin of a piece of resolution provenance (e.g. a source
/// document, dataset, or repository). Purely descriptive provenance data; it
/// carries no interpretation or fiscal conclusion.
/// </summary>
public sealed record ProvenanceSource
{
    public string Value { get; }

    public ProvenanceSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Provenance source cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
