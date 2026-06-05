using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionProvenanceEngine : ILegalReferenceResolutionProvenanceEngine
{
    private readonly ILegalReferenceResolutionProvenancePipeline _pipeline;

    public LegalReferenceResolutionProvenanceEngine(ILegalReferenceResolutionProvenancePipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        _pipeline = pipeline;
    }

    public async Task<ProvenanceChain> BuildAsync(
        IReadOnlyList<ResolutionProvenance> provenances,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provenances);

        var context = new LegalReferenceResolutionProvenanceContext(provenances);
        var processed = await _pipeline.RunAsync(context, cancellationToken);
        var steps = processed.Provenances
            .SelectMany(provenance => provenance.Chain.Steps)
            .ToList();

        return new ProvenanceChain(steps);
    }
}
