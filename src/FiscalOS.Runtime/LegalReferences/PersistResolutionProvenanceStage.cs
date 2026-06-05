using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class PersistResolutionProvenanceStage : ILegalReferenceResolutionProvenanceStage
{
    private readonly ILegalReferenceResolutionProvenanceRepository _repository;

    public PersistResolutionProvenanceStage(ILegalReferenceResolutionProvenanceRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        _repository = repository;
    }

    public string Name => "persist-resolution-provenance";

    public async Task<LegalReferenceResolutionProvenanceContext> ExecuteAsync(
        LegalReferenceResolutionProvenanceContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var provenance in context.Provenances)
        {
            await _repository.StoreAsync(provenance, cancellationToken);
        }

        return context;
    }
}
