using System.Collections.Generic;

namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionResult(
    IngestionBatchId BatchId,
    IngestionStatus Status,
    IReadOnlyList<IngestionTraceEntry> Trace)
{
    public bool IsSuccessful => Status == IngestionStatus.Succeeded;
}
