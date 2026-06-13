using System.Collections.Generic;

namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionContext
{
    public IngestionContext(
        IngestionBatchId batchId,
        IReadOnlyList<RawLegislationDocument> documents,
        IReadOnlyList<IngestionTraceEntry> trace)
        : this(
            batchId,
            documents,
            trace,
            Array.Empty<IngestionProvenanceRecord>(),
            Array.Empty<IngestionAuditEventRecord>())
    {
    }

    public IngestionContext(
        IngestionBatchId batchId,
        IReadOnlyList<RawLegislationDocument> documents,
        IReadOnlyList<IngestionTraceEntry> trace,
        IReadOnlyList<IngestionProvenanceRecord> provenance,
        IReadOnlyList<IngestionAuditEventRecord> auditEvents)
    {
        ArgumentNullException.ThrowIfNull(batchId);
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(trace);
        ArgumentNullException.ThrowIfNull(provenance);
        ArgumentNullException.ThrowIfNull(auditEvents);

        BatchId = batchId;
        Documents = documents;
        Trace = trace;
        Provenance = provenance;
        AuditEvents = auditEvents;
    }

    public IngestionBatchId BatchId { get; init; }

    public IReadOnlyList<RawLegislationDocument> Documents { get; init; }

    public IReadOnlyList<IngestionTraceEntry> Trace { get; init; }

    public IReadOnlyList<IngestionProvenanceRecord> Provenance { get; init; }

    public IReadOnlyList<IngestionAuditEventRecord> AuditEvents { get; init; }
}
