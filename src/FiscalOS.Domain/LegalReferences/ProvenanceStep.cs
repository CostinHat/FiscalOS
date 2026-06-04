namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A single step in the derivation of a resolution: the <see cref="ProvenanceSource"/>
/// it came from and a human-readable description of what it contributed. Pure
/// provenance data; no resolution logic.
/// </summary>
public sealed record ProvenanceStep
{
    public ProvenanceSource Source { get; }

    public string Description { get; }

    public ProvenanceStep(ProvenanceSource source, string description)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Provenance step description cannot be empty.", nameof(description));
        }

        Source = source;
        Description = description.Trim();
    }
}
