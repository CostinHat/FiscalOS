namespace FiscalOS.Domain.LegislationIngestion;

public interface IRawLegislationDocumentRepository
{
    Task StoreAsync(RawLegislationDocument document, CancellationToken cancellationToken = default);

    Task<RawLegislationDocument?> GetAsync(LegislationDocumentId id, CancellationToken cancellationToken = default);
}
