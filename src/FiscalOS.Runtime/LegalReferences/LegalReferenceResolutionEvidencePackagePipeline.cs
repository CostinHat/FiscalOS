using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionEvidencePackagePipeline : ILegalReferenceResolutionEvidencePackagePipeline
{
    private readonly IReadOnlyList<ILegalReferenceResolutionEvidencePackageStage> _stages;

    public LegalReferenceResolutionEvidencePackagePipeline(params ILegalReferenceResolutionEvidencePackageStage[] stages)
        : this((IReadOnlyList<ILegalReferenceResolutionEvidencePackageStage>)stages)
    {
    }

    public LegalReferenceResolutionEvidencePackagePipeline(IReadOnlyList<ILegalReferenceResolutionEvidencePackageStage> stages)
    {
        ArgumentNullException.ThrowIfNull(stages);

        _stages = stages.ToArray();
    }

    public async Task<LegalReferenceResolutionEvidencePackageContext> RunAsync(
        LegalReferenceResolutionEvidencePackageContext context,
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
