namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaLegalReferenceOperationalEvent(
    string Name,
    string? CorrelationId,
    string? TenantId,
    string? AccountId,
    string? WorkflowId,
    IContaLegalReferenceStatus? Status,
    bool? IncludeTraceability,
    string Outcome);
