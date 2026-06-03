namespace FiscalOS.LegalKnowledge;

public sealed record AuditEvent(
    string EventType,
    DateTimeOffset Timestamp,
    string Description);
