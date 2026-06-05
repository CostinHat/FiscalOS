using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionPipeline : ILegalReferenceResolutionPipeline
{
    private readonly IReadOnlyList<ILegalReferenceResolutionStage> _stages;

    public LegalReferenceResolutionPipeline(params ILegalReferenceResolutionStage[] stages)
        : this((IReadOnlyList<ILegalReferenceResolutionStage>)stages)
    {
    }

    public LegalReferenceResolutionPipeline(IReadOnlyList<ILegalReferenceResolutionStage> stages)
    {
        ArgumentNullException.ThrowIfNull(stages);

        _stages = stages.ToArray();
    }

    public async Task<LegalReferenceResolutionContext> RunAsync(
        LegalReferenceResolutionContext context,
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
