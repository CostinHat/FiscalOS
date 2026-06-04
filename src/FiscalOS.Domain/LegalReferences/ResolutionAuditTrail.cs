using System.Collections.Generic;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// An ordered, immutable collection of <see cref="ResolutionAuditEntry"/> records
/// describing how resolution decisions were reached. Pure audit data; it carries
/// no resolution logic.
/// </summary>
public sealed record ResolutionAuditTrail(
    IReadOnlyList<ResolutionAuditEntry> Entries)
{
    public bool IsEmpty => Entries.Count == 0;
}
