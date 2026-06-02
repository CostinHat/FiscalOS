using FiscalOS.Core.Classification;

namespace FiscalOS.Runtime.Classification;

public sealed record KnowledgeProjectionResult(
    IReadOnlyCollection<ExplanationNode> Atoms,
    IReadOnlyCollection<ExplanationEdge> Relationships,
    ExplanationNarrative Narrative);
