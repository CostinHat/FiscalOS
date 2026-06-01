using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public sealed record PurposeChain(
    LegalCitation GoverningCitation,
    IReadOnlyList<PurposeNode> Nodes)
{
    public PurposeNode Leaf => Nodes[^1];

    public int Depth => Nodes.Count;
}
