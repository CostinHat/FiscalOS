namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaOperationalMetadata(
    string? TenantId,
    string? AccountId,
    string? WorkflowId);
