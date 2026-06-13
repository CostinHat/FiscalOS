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
            Array.Empty<RawDocumentIdentityDecision>(),
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
        : this(
            batchId,
            documents,
            trace,
            Array.Empty<RawDocumentIdentityDecision>(),
            provenance,
            auditEvents)
    {
    }

    public IngestionContext(
        IngestionBatchId batchId,
        IReadOnlyList<RawLegislationDocument> documents,
        IReadOnlyList<IngestionTraceEntry> trace,
        IReadOnlyList<RawDocumentIdentityDecision> rawDocumentIdentityDecisions,
        IReadOnlyList<IngestionProvenanceRecord> provenance,
        IReadOnlyList<IngestionAuditEventRecord> auditEvents)
    {
        ArgumentNullException.ThrowIfNull(batchId);
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(trace);
        ArgumentNullException.ThrowIfNull(rawDocumentIdentityDecisions);
        ArgumentNullException.ThrowIfNull(provenance);
        ArgumentNullException.ThrowIfNull(auditEvents);

        BatchId = batchId;
        Documents = documents;
        Trace = trace;
        RawDocumentIdentityDecisions = rawDocumentIdentityDecisions;
        Provenance = provenance;
        AuditEvents = auditEvents;
    }

    public IngestionBatchId BatchId { get; init; }

    public IReadOnlyList<RawLegislationDocument> Documents { get; init; }

    public IReadOnlyList<IngestionTraceEntry> Trace { get; init; }

    public IReadOnlyList<RawDocumentIdentityDecision> RawDocumentIdentityDecisions { get; init; }

    public IReadOnlyList<IngestionProvenanceRecord> Provenance { get; init; }

    public IReadOnlyList<IngestionAuditEventRecord> AuditEvents { get; init; }
}
