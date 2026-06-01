namespace FiscalOS.Core.Classification;

public sealed record ExplanationGraph(
    IReadOnlyCollection<ExplanationNode> Nodes,
    IReadOnlyCollection<ExplanationEdge> Edges);

public sealed record ExplanationNode(
    string Id,
    string Label,
    string Type);

public sealed record ExplanationEdge(
    string FromNodeId,
    string ToNodeId,
    string Label);