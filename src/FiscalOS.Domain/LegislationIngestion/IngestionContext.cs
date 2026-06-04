using System.Collections.Generic;

namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionContext(
    IngestionBatchId BatchId,
    IReadOnlyList<RawLegislationDocument> Documents,
    IReadOnlyList<IngestionTraceEntry> Trace);
