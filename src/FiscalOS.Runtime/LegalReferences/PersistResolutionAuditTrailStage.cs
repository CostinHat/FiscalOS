using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class PersistResolutionAuditTrailStage : ILegalReferenceResolutionAuditStage
{
    private readonly ILegalReferenceResolutionAuditRepository _repository;

    public PersistResolutionAuditTrailStage(ILegalReferenceResolutionAuditRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        _repository = repository;
    }

    public string Name => "persist-resolution-audit-trail";

    public async Task<LegalReferenceResolutionAuditContext> ExecuteAsync(
        LegalReferenceResolutionAuditContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        await _repository.StoreAsync(new ResolutionAuditTrail(context.Entries), cancellationToken);
        return context;
    }
}
