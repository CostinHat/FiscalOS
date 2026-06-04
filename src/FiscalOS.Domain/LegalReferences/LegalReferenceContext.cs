using System.Collections.Generic;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The working state flowing through a legal reference pipeline: the set of
/// structural <see cref="FullyQualifiedLegalReference"/> addresses being processed.
/// It carries no legal content, interpretation, or fiscal conclusions.
/// </summary>
public sealed record LegalReferenceContext(
    IReadOnlyList<FullyQualifiedLegalReference> References);
