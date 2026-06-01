using System.Collections.Generic;
using System.Linq;
using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime.Classification;

public static class ExplanationNarrativeProjector
{
    public static ExplanationNarrative Project(DecisionExplanation explanation)
    {
        ArgumentNullException.ThrowIfNull(explanation);

        var lines = new List<string>
        {
            OutcomeLine(explanation),
            LegalBasisLine(explanation.LegalBasis),
            ProvenanceLine(explanation.AuditGraph)
        };

        return new ExplanationNarrative(lines);
    }

    private static string OutcomeLine(DecisionExplanation explanation)
    {
        var category = explanation.AuditGraph
            .NodesOfType(AuditNodeType.Decision)
            .FirstOrDefault()?.Description ?? "Unknown";

        return $"Decision: {category}.";
    }

    private static string LegalBasisLine(DecisionLegalBasis legalBasis)
    {
        if (legalBasis.IsEmpty)
        {
            return "Legal basis: none established.";
        }

        var count = legalBasis.GoverningCitations.Count;

        return legalBasis.IsResolved
            ? $"Legal basis: resolved to {count} governing citation(s)."
            : $"Legal basis: unresolved conflict among {count} competing citation(s).";
    }

    private static string ProvenanceLine(AuditGraph auditGraph)
    {
        var ruleCount = auditGraph.NodesOfType(AuditNodeType.Rule).Count;
        var conflictApplied = auditGraph.NodesOfType(AuditNodeType.ConflictResolution).Any();

        var line = $"Provenance: {ruleCount} rule(s) evaluated";

        return conflictApplied
            ? line + ", conflict resolution applied."
            : line + ".";
    }
}
