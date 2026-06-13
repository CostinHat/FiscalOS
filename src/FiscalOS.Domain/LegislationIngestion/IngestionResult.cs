using System.Collections.Generic;

namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionResult
{
    public IngestionResult(
        IngestionBatchId batchId,
        IngestionStatus status,
        IReadOnlyList<IngestionTraceEntry> trace)
        : this(
            batchId,
            status,
            trace,
            Array.Empty<RawDocumentIdentityDecision>(),
            Array.Empty<IngestionProvenanceRecord>(),
            Array.Empty<IngestionAuditEventRecord>())
    {
    }

    public IngestionResult(
        IngestionBatchId batchId,
        IngestionStatus status,
        IReadOnlyList<IngestionTraceEntry> trace,
        IReadOnlyList<IngestionProvenanceRecord> provenance,
        IReadOnlyList<IngestionAuditEventRecord> auditEvents)
        : this(
            batchId,
            status,
            trace,
            Array.Empty<RawDocumentIdentityDecision>(),
            provenance,
            auditEvents)
    {
    }

    public IngestionResult(
        IngestionBatchId batchId,
        IngestionStatus status,
        IReadOnlyList<IngestionTraceEntry> trace,
        IReadOnlyList<RawDocumentIdentityDecision> rawDocumentIdentityDecisions,
        IReadOnlyList<IngestionProvenanceRecord> provenance,
        IReadOnlyList<IngestionAuditEventRecord> auditEvents)
    {
        ArgumentNullException.ThrowIfNull(batchId);
        ArgumentNullException.ThrowIfNull(trace);
        ArgumentNullException.ThrowIfNull(rawDocumentIdentityDecisions);
        ArgumentNullException.ThrowIfNull(provenance);
        ArgumentNullException.ThrowIfNull(auditEvents);

        BatchId = batchId;
        Status = status;
        Trace = trace;
        RawDocumentIdentityDecisions = rawDocumentIdentityDecisions;
        Provenance = provenance;
        AuditEvents = auditEvents;
    }

    public IngestionBatchId BatchId { get; init; }

    public IngestionStatus Status { get; init; }

    public IReadOnlyList<IngestionTraceEntry> Trace { get; init; }

    public IReadOnlyList<RawDocumentIdentityDecision> RawDocumentIdentityDecisions { get; init; }

    public IReadOnlyList<IngestionProvenanceRecord> Provenance { get; init; }

    public IReadOnlyList<IngestionAuditEventRecord> AuditEvents { get; init; }

    public bool IsSuccessful => Status == IngestionStatus.Succeeded;
}
