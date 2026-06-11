using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences.Embedded;

public sealed record EmbeddedLegalReferenceRequest(
    LegalReference Reference,
    string CorrelationId,
    bool IncludeTraceability);
