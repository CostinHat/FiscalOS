namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaLegalReferenceRequest(
    IReadOnlyList<IContaLegalReferenceSegment> Segments,
    string CorrelationId,
    bool IncludeTraceability,
    IContaAuthorizationContext Authorization,
    IContaOperationalMetadata OperationalMetadata);
