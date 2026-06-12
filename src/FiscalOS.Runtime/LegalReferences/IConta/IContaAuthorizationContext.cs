namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaAuthorizationContext(
    bool IsAuthorized,
    string? DenialReason,
    string? UserId,
    string? TenantId,
    string? AccountId,
    string? WorkflowId);
