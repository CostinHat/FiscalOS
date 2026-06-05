using System.Collections.Generic;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The working state flowing through a resolution evidence package pipeline: the
/// <see cref="ResolutionEvidencePackage"/> records accumulated so far. Pure data;
/// it carries no resolution, audit, or provenance logic, and composes, rather
/// than merges, those concerns.
/// </summary>
public sealed record LegalReferenceResolutionEvidencePackageContext(
    IReadOnlyList<ResolutionEvidencePackage> Packages);
