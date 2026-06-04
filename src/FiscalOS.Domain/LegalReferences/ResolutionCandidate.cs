namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A single candidate produced when resolving a structural legal address: a
/// concrete <see cref="FullyQualifiedLegalReference"/> the query may refer to.
/// It carries no score, ranking, or interpretation.
/// </summary>
public sealed record ResolutionCandidate
{
    public FullyQualifiedLegalReference Reference { get; }

    public ResolutionCandidate(FullyQualifiedLegalReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        Reference = reference;
    }
}
