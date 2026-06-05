using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionEvidencePackageEngine : ILegalReferenceResolutionEvidencePackageEngine
{
    private readonly ILegalReferenceResolutionEvidencePackagePipeline _pipeline;

    public LegalReferenceResolutionEvidencePackageEngine(ILegalReferenceResolutionEvidencePackagePipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        _pipeline = pipeline;
    }

    public async Task<IReadOnlyList<ResolutionEvidencePackage>> ProcessAsync(
        IReadOnlyList<ResolutionEvidencePackage> packages,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packages);

        var context = new LegalReferenceResolutionEvidencePackageContext(packages);
        var processed = await _pipeline.RunAsync(context, cancellationToken);

        return processed.Packages;
    }
}
