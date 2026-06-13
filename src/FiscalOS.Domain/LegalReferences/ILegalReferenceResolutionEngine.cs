using FiscalOS.LegalCore;

namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The top-level contract for resolving structural <see cref="LegalReference"/>
/// queries into <see cref="ResolutionResult"/> outcomes. It is a pure facade; no
/// resolution algorithm, search engine, persistence, or interpretation is
/// prescribed.
/// </summary>
public interface ILegalReferenceResolutionEngine
{
    Task<IReadOnlyList<ResolutionResult>> ResolveAsync(
        IReadOnlyList<LegalReference> references,
        CancellationToken cancellationToken = default);
}
