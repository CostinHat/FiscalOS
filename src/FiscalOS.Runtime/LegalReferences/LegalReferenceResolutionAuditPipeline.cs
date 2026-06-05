using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionAuditPipeline : ILegalReferenceResolutionAuditPipeline
{
    private readonly IReadOnlyList<ILegalReferenceResolutionAuditStage> _stages;

    public LegalReferenceResolutionAuditPipeline(params ILegalReferenceResolutionAuditStage[] stages)
        : this((IReadOnlyList<ILegalReferenceResolutionAuditStage>)stages)
    {
    }

    public LegalReferenceResolutionAuditPipeline(IReadOnlyList<ILegalReferenceResolutionAuditStage> stages)
    {
        ArgumentNullException.ThrowIfNull(stages);

        _stages = stages.ToArray();
    }

    public async Task<LegalReferenceResolutionAuditContext> RunAsync(
        LegalReferenceResolutionAuditContext context,
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
