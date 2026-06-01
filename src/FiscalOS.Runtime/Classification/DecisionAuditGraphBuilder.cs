using System.Collections.Generic;
using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public static class DecisionAuditGraphBuilder
{
    private const string DecisionNodeId = "decision";
    private const string ConflictNodeId = "conflict";

    public static AuditGraph Build(
        IReadOnlyList<Evaluation.RuleEvaluationResult> evaluations,
        Evaluation.RuleEvaluationResult? winningEvaluation,
        Core.Classification.ClassificationResult result,
        DecisionLegalBasis legalBasis)
    {
        ArgumentNullException.ThrowIfNull(evaluations);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(legalBasis);

        var nodes = new List<AuditNode>
        {
            new(DecisionNodeId, AuditNodeType.Decision, result.Category)
        };
        var edges = new List<AuditEdge>();

        foreach (var evaluation in evaluations)
        {
            nodes.Add(new AuditNode(
                RuleNodeId(evaluation.RuleId),
                AuditNodeType.Rule,
                $"{evaluation.RuleId} (passed={evaluation.Passed}, category={evaluation.Category ?? "none"})"));
        }

        if (winningEvaluation is not null)
        {
            edges.Add(new AuditEdge(
                RuleNodeId(winningEvaluation.RuleId),
                DecisionNodeId,
                AuditEdgeType.Produces));
        }

        if (!legalBasis.IsEmpty)
        {
            var description = legalBasis.IsResolved
                ? $"Resolved: {legalBasis.GoverningCitations.Count} governing citation(s)."
                : $"Unresolved: {legalBasis.GoverningCitations.Count} competing citation(s).";

            nodes.Add(new AuditNode(ConflictNodeId, AuditNodeType.ConflictResolution, description));
            edges.Add(new AuditEdge(ConflictNodeId, DecisionNodeId, AuditEdgeType.Justifies));
        }

        return new AuditGraph(nodes, edges);
    }

    private static string RuleNodeId(string ruleId) => $"rule:{ruleId}";
}
