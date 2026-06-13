using System.Collections.Generic;
using FiscalOS.LegalCore;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The working state flowing through a legal reference resolution pipeline: the
/// structural <see cref="LegalReference"/> queries to resolve and the
/// <see cref="ResolutionResult"/> outcomes accumulated so far. Pure data; it
/// carries no resolution logic, search, or interpretation.
/// </summary>
public sealed record LegalReferenceResolutionContext(
    IReadOnlyList<LegalReference> Queries,
    IReadOnlyList<ResolutionResult> Results);
