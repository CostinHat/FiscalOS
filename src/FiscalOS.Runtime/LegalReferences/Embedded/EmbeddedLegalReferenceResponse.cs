using FiscalOS.Runtime.LegalReferences.Traceability;

namespace FiscalOS.Runtime.LegalReferences.Embedded;

public sealed record EmbeddedLegalReferenceResponse(
    EmbeddedLegalReferenceStatus Status,
    string? Answer,
    PublicLegalCitation? Citation,
    string Explanation,
    string CorrelationId,
    TraceabilitySummary? TraceabilitySummary);
