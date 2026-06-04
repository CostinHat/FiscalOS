using System.Collections.Generic;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The working state flowing through a resolution audit pipeline: the
/// <see cref="ResolutionAuditEntry"/> records accumulated so far. Pure data; it
/// carries no audit logic, resolution logic, or interpretation.
/// </summary>
public sealed record LegalReferenceResolutionAuditContext(
    IReadOnlyList<ResolutionAuditEntry> Entries);
