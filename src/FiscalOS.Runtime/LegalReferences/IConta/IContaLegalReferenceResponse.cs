namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaLegalReferenceResponse(
    IContaLegalReferenceStatus Status,
    string? Answer,
    IContaLegalCitation? Citation,
    string Explanation,
    string CorrelationId,
    IContaTraceabilitySummary? TraceabilitySummary);
