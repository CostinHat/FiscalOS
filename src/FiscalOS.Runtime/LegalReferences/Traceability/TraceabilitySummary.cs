namespace FiscalOS.Runtime.LegalReferences.Traceability;

public sealed record TraceabilitySummary(
    string CorrelationId,
    PublicLegalReference RequestedReference,
    TraceabilityStatus Status,
    PublicLegalCitation? Citation,
    string? SourceSummary,
    string Linkage,
    IReadOnlyList<string> Limitations);
