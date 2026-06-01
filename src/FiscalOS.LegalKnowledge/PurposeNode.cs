namespace FiscalOS.LegalKnowledge;

public sealed record PurposeNode(
    string Id,
    PurposeNodeType Kind,
    string Description,
    LegalCitation? Citation);
