namespace FiscalOS.LegalKnowledge;

public sealed record AuditEdge(
    string FromNodeId,
    string ToNodeId,
    AuditEdgeType Type);
