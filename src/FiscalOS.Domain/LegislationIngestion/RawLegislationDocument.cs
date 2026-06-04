namespace FiscalOS.Domain.LegislationIngestion;

public sealed record RawLegislationDocument(
    LegislationDocumentId Id,
    LegislationSourceReference Source,
    string Content);
