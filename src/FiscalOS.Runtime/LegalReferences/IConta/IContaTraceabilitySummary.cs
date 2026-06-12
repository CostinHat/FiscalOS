namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaTraceabilitySummary(
    string CorrelationId,
    IContaPublicLegalReference RequestedReference,
    IContaLegalReferenceStatus Status,
    IContaLegalCitation? Citation,
    string? SourceSummary,
    string Linkage,
    IReadOnlyList<string> Limitations);
