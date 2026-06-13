using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class DefaultIngestionEmissionIdPolicy : IIngestionEmissionIdPolicy
{
    public IngestionProvenanceId CreateProvenanceId(IngestionBatchId batchId, string emissionName)
    {
        ArgumentNullException.ThrowIfNull(batchId);

        if (string.IsNullOrWhiteSpace(emissionName))
        {
            throw new ArgumentException("Ingestion emission name cannot be empty.", nameof(emissionName));
        }

        return new IngestionProvenanceId($"{batchId.Value}:provenance:{emissionName.Trim()}");
    }

    public IngestionAuditEventId CreateAuditEventId(IngestionBatchId batchId, string emissionName)
    {
        ArgumentNullException.ThrowIfNull(batchId);

        if (string.IsNullOrWhiteSpace(emissionName))
        {
            throw new ArgumentException("Ingestion emission name cannot be empty.", nameof(emissionName));
        }

        return new IngestionAuditEventId($"{batchId.Value}:audit:{emissionName.Trim()}");
    }
}
