namespace FiscalOS.LegalKnowledge;

public sealed record AuditNode(
    string Id,
    AuditNodeType Type,
    string Description);
