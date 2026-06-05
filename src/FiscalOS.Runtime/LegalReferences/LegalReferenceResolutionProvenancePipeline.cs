using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionProvenancePipeline : ILegalReferenceResolutionProvenancePipeline
{
    private readonly IReadOnlyList<ILegalReferenceResolutionProvenanceStage> _stages;

    public LegalReferenceResolutionProvenancePipeline(params ILegalReferenceResolutionProvenanceStage[] stages)
        : this((IReadOnlyList<ILegalReferenceResolutionProvenanceStage>)stages)
    {
    }

    public LegalReferenceResolutionProvenancePipeline(IReadOnlyList<ILegalReferenceResolutionProvenanceStage> stages)
    {
        ArgumentNullException.ThrowIfNull(stages);

        _stages = stages.ToArray();
    }

    public async Task<LegalReferenceResolutionProvenanceContext> RunAsync(
        LegalReferenceResolutionProvenanceContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var current = context;
        foreach (var stage in _stages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            current = await stage.ExecuteAsync(current, cancellationToken);
        }

        return current;
    }
}
