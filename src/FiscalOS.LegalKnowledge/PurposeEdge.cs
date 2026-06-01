namespace FiscalOS.LegalKnowledge;

public sealed record PurposeEdge(
    string FromNodeId,
    string ToNodeId,
    PurposeRelationType Relation);
