using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public interface IIngestionEmissionIdPolicy
{
    IngestionProvenanceId CreateProvenanceId(IngestionBatchId batchId, string emissionName);

    IngestionAuditEventId CreateAuditEventId(IngestionBatchId batchId, string emissionName);
}
