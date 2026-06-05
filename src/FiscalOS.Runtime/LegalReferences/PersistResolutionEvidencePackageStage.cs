using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class PersistResolutionEvidencePackageStage : ILegalReferenceResolutionEvidencePackageStage
{
    private readonly ILegalReferenceResolutionEvidencePackageRepository _repository;

    public PersistResolutionEvidencePackageStage(ILegalReferenceResolutionEvidencePackageRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        _repository = repository;
    }

    public string Name => "persist-resolution-evidence-package";

    public async Task<LegalReferenceResolutionEvidencePackageContext> ExecuteAsync(
        LegalReferenceResolutionEvidencePackageContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var package in context.Packages)
        {
            await _repository.StoreAsync(package.Provenance.Query, package, cancellationToken);
        }

        return context;
    }
}
