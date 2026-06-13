using System.Linq;
using System;
using FiscalOS.Core;
using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification.Rules;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Evaluation;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Attach_Audit_Graph_To_Decision
{
    private static LegalCitation Citation(
        LegalSourceType sourceType,
        SpecificityLevel specificity,
        DateOnly effectiveDate,
        string article) =>
        new(sourceType, "Legea 227/2015", article, specificity, effectiveDate, new JurisdictionId("RO"));

    private static ClassificationEngine Engine(params ClassificationRule[] rules) =>
        new(new DefaultRuleRegistry(rules));

    private sealed class StubRule : ClassificationRule
    {
        private readonly bool _passes;
        private readonly IReadOnlyList<LegalCitation> _citations;

        public StubRule(string ruleId, int priority, bool passes, params LegalCitation[] citations)
        {
            RuleId = ruleId;
            Priority = priority;
            _passes = passes;
            _citations = citations;
        }

        public string RuleId { get; }

        public string Description => "Test stub rule.";

        public int Priority { get; }

        public RuleEvaluationResult Evaluate(ClassificationContext context) =>
            new(RuleId, _passes, "stub", _passes ? "StubCategory" : null) { Citations = _citations };
    }

    [Fact]
    public async Task Decision_exposes_an_audit_graph_with_a_decision_node()
    {
        var engine = Engine(new StubRule("R1", 100, passes: true));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.NotNull(decision.Explanation.AuditGraph);
        var decisionNode = decision.Explanation.AuditGraph.NodeById("decision");
        Assert.NotNull(decisionNode);
        Assert.Equal(AuditNodeType.Decision, decisionNode!.Type);
        Assert.Equal("StubCategory", decisionNode.Description);
    }

    [Fact]
    public async Task Every_evaluated_rule_becomes_a_node_and_the_winner_links_to_the_decision()
    {
        var winner = new StubRule("WINNER", 100, passes: true);
        var loser = new StubRule("LOSER", 50, passes: false);
        var engine = Engine(winner, loser);

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.Equal(2, decision.Explanation.AuditGraph.NodesOfType(AuditNodeType.Rule).Count);

        var winnerEdge = Assert.Single(decision.Explanation.AuditGraph.EdgesFrom("rule:WINNER"));
        Assert.Equal("decision", winnerEdge.ToNodeId);
        Assert.Equal(AuditEdgeType.Produces, winnerEdge.Type);

        // The non-winning rule is recorded as a node but is not linked to the decision.
        Assert.NotNull(decision.Explanation.AuditGraph.NodeById("rule:LOSER"));
        Assert.Empty(decision.Explanation.AuditGraph.EdgesFrom("rule:LOSER"));
    }

    [Fact]
    public async Task Winning_rule_with_citations_adds_a_resolved_conflict_node()
    {
        var citation = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 1");
        var engine = Engine(new StubRule("R1", 100, passes: true, citation));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        var conflict = decision.Explanation.AuditGraph.NodeById("conflict");
        Assert.NotNull(conflict);
        Assert.Equal(AuditNodeType.ConflictResolution, conflict!.Type);
        Assert.Contains("Resolved", conflict.Description);

        var justifies = Assert.Single(decision.Explanation.AuditGraph.EdgesFrom("conflict"));
        Assert.Equal("decision", justifies.ToNodeId);
        Assert.Equal(AuditEdgeType.Justifies, justifies.Type);
    }

    [Fact]
    public async Task Unresolved_conflict_node_reflects_the_unresolved_basis()
    {
        var a = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 1");
        var b = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 2");
        var engine = Engine(new StubRule("R1", 100, passes: true, a, b));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.True(decision.Explanation.HasUnresolvedLegalConflict);
        Assert.Contains("Unresolved", decision.Explanation.AuditGraph.NodeById("conflict")!.Description);
        Assert.Equal("StubCategory", decision.Result.Category);
    }

    [Fact]
    public async Task Winning_rule_without_citations_has_no_conflict_node()
    {
        var engine = Engine(new StubRule("R1", 100, passes: true));

        var decision = await engine.ClassifyAsync(new FiscalSubject());

        Assert.Equal("R1", decision.WinningRuleId);
        Assert.Null(decision.Explanation.AuditGraph.NodeById("conflict"));
        Assert.Empty(decision.Explanation.AuditGraph.NodesOfType(AuditNodeType.ConflictResolution));
    }

    [Fact]
    public async Task No_winning_rule_produces_an_unclassified_decision_node_and_no_conflict_node()
    {
        var ineligible = new FiscalSubject { Revenue = 600_000m, EmployeeCount = 3 };
        var engine = Engine(new MicroenterpriseClassificationRule());

        var decision = await engine.ClassifyAsync(ineligible);

        Assert.Null(decision.WinningRuleId);
        Assert.Equal("Unclassified", decision.Explanation.AuditGraph.NodeById("decision")!.Description);
        Assert.Null(decision.Explanation.AuditGraph.NodeById("conflict"));
        Assert.Single(decision.Explanation.AuditGraph.NodesOfType(AuditNodeType.Rule));
    }

    [Fact]
    public async Task Duplicate_rule_ids_fail_fast()
    {
        var engine = Engine(
            new StubRule("DUP", 100, passes: true),
            new StubRule("DUP", 50, passes: false));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => engine.ClassifyAsync(new FiscalSubject()));
    }

    [Fact]
    public async Task Audit_graph_is_deterministic_across_runs()
    {
        ClassificationEngine MakeEngine() => Engine(
            new StubRule("R1", 100, passes: true),
            new StubRule("R2", 50, passes: false));

        var first = await MakeEngine().ClassifyAsync(new FiscalSubject());
        var second = await MakeEngine().ClassifyAsync(new FiscalSubject());

        Assert.Equal(
            first.Explanation.AuditGraph.Nodes.Select(n => (n.Id, n.Type, n.Description)),
            second.Explanation.AuditGraph.Nodes.Select(n => (n.Id, n.Type, n.Description)));
        Assert.Equal(
            first.Explanation.AuditGraph.Edges.Select(e => (e.FromNodeId, e.ToNodeId, e.Type)),
            second.Explanation.AuditGraph.Edges.Select(e => (e.FromNodeId, e.ToNodeId, e.Type)));
    }
}
