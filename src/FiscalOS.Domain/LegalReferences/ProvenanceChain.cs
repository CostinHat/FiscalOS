using System.Collections.Generic;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// An ordered, immutable sequence of <see cref="ProvenanceStep"/>s describing how
/// a resolution was derived. Pure provenance data; it carries no resolution logic.
/// </summary>
public sealed record ProvenanceChain(
    IReadOnlyList<ProvenanceStep> Steps)
{
    public bool IsEmpty => Steps.Count == 0;
}
