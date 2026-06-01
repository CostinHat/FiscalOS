namespace FiscalOS.LegalKnowledge;

public sealed record LegalCitation(
    LegalSourceType SourceType,
    string SourceReference,
    string Article,
    SpecificityLevel Specificity,
    DateOnly EffectiveDate);
