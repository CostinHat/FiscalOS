using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public sealed record PurposeReference(
    LegalCitation GoverningCitation,
    PurposeNode PurposeNode);
