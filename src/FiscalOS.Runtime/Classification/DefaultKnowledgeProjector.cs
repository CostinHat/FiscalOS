using System.Collections.Generic;
using FiscalOS.Core.Classification;

namespace FiscalOS.Runtime.Classification;

public sealed class DefaultKnowledgeProjector : IKnowledgeProjector
{
    private const string DecisionNodeId = "decision";

    public KnowledgeProjectionResult Project(ClassificationDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        var nodes = new List<ExplanationNode>
        {
            new(DecisionNodeId, decision.Result.Category, "Decision")
        };
        var edges = new List<ExplanationEdge>();

        var governingCitations = decision.Explanation.LegalBasis.GoverningCitations;
        for (var i = 0; i < governingCitations.Count; i++)
        {
            var citation = governingCitations[i];
            var citationNodeId = $"citation:{i}";

            nodes.Add(new ExplanationNode(
                citationNodeId,
                $"{citation.SourceType} {citation.SourceReference} {citation.Article} ({citation.Jurisdiction.Value})",
                "LegalBasis"));
            edges.Add(new ExplanationEdge(DecisionNodeId, citationNodeId, "based-on"));
        }

        var narrative = ExplanationNarrativeProjector.Project(decision.Explanation);

        return new KnowledgeProjectionResult(nodes, edges, narrative);
    }
}
