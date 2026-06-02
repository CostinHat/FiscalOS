using FiscalOS.Core.Classification;

namespace FiscalOS.Runtime.Classification;

public sealed class DefaultKnowledgeProjector : IKnowledgeProjector
{
    private const string DecisionAtomId = "decision";
    private const string PurposeAtomId = "purpose";

    public KnowledgeProjectionResult Project(ClassificationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var decisionAtom = new ExplanationNode(DecisionAtomId, result.Category, "Decision");
        var purposeAtom = new ExplanationNode(PurposeAtomId, result.Explanation, "Purpose");
        var link = new ExplanationEdge(DecisionAtomId, PurposeAtomId, "explained-by");

        var narrative = new ExplanationNarrative(new[]
        {
            $"Decision: {result.Category}.",
            $"Purpose: {result.Explanation}."
        });

        return new KnowledgeProjectionResult(
            new[] { decisionAtom, purposeAtom },
            new[] { link },
            narrative);
    }
}
