using FiscalOS.LegalCore;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// A structural legal address query that could not be resolved to a concrete
/// reference, together with a human-readable reason. Purely descriptive; it
/// carries no interpretation or fiscal conclusion.
/// </summary>
public sealed record UnresolvedReference
{
    public LegalReference Query { get; }

    public string Reason { get; }

    public UnresolvedReference(LegalReference query, string reason)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Unresolved reference reason cannot be empty.", nameof(reason));
        }

        Query = query;
        Reason = reason.Trim();
    }
}
