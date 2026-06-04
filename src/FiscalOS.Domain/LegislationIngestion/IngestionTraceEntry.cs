namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionTraceEntry(
    IngestionStage Stage,
    IngestionStatus Status,
    DateTimeOffset Timestamp,
    string Description);
