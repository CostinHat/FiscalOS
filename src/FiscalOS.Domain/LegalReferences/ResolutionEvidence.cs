namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A human-readable justification recorded for a resolution decision. Purely
/// descriptive audit data; it carries no interpretation or fiscal conclusion.
/// </summary>
public sealed record ResolutionEvidence
{
    public string Description { get; }

    public ResolutionEvidence(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Resolution evidence description cannot be empty.", nameof(description));
        }

        Description = description.Trim();
    }

    public override string ToString()
    {
        return Description;
    }
}
