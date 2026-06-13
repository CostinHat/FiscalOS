using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;

namespace FiscalOS.Runtime.LegalReferences.Embedded;

public sealed record EmbeddedLegalReferenceRequest(
    LegalReference Reference,
    string CorrelationId,
    bool IncludeTraceability);
