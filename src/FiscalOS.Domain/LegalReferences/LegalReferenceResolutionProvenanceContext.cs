using System.Collections.Generic;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The working state flowing through a resolution provenance pipeline: the
/// <see cref="ResolutionProvenance"/> records accumulated so far. Pure data; it
/// carries no provenance logic, resolution logic, or interpretation.
/// </summary>
public sealed record LegalReferenceResolutionProvenanceContext(
    IReadOnlyList<ResolutionProvenance> Provenances);
