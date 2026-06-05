namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// The top-level contract for assembling a single <see cref="ProvenanceChain"/>
/// from a set of <see cref="ResolutionProvenance"/> records. It is a pure facade;
/// no provenance algorithm, persistence, or interpretation is prescribed.
/// </summary>
public interface ILegalReferenceResolutionProvenanceEngine
{
    Task<ProvenanceChain> BuildAsync(
        IReadOnlyList<ResolutionProvenance> provenances,
        CancellationToken cancellationToken = default);
}
