using System;
using System.Linq;
using FiscalOS.Core;
using FiscalOS.Core.Classification;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Project_Classification_To_Knowledge_Model
{
    private static LegalCitation Citation(string article) =>
        new(LegalSourceType.Law, "Legea 227/2015", article, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), new JurisdictionId("RO"));

    private sealed class CitingStubRule : ClassificationRule
    {
        private readonly IReadOnlyList<LegalCitation> _citations;

        public CitingStubRule(params LegalCitation[] citations) => _citations = citations;

        public string RuleId => "STUB_RULE";

        public string Description => "Test stub that emits legal citations.";

        public int Priority => 100;

        public FiscalOS.Runtime.Evaluation.RuleEvaluationResult Evaluate(ClassificationContext context) =>
            new(RuleId, true, "stub passed", "StubCategory") { Citations = _citations };
    }

    private static async Task<ClassificationDecision> DecisionWith(params LegalCitation[] citations)
    {
        var engine = new ClassificationEngine(
            new DefaultRuleRegistry(new ClassificationRule[] { new CitingStubRule(citations) }));
        return await engine.ClassifyAsync(new FiscalSubject());
    }

    [Fact]
    public async Task Projects_a_decision_node_from_the_classification_outcome()
    {
        var decision = await DecisionWith(Citation("Art. 1"));

        var projection = new DefaultKnowledgeProjector().Project(decision);

        var decisionNode = Assert.Single(projection.Nodes, n => n.Id == "decision");
        Assert.Equal("Decision", decisionNode.Type);
        Assert.Equal("StubCategory", decisionNode.Label);
    }

    [Fact]
    public async Task Projects_a_legal_basis_node_and_edge_per_governing_citation()
    {
        var decision = await DecisionWith(Citation("Art. 1"));

        var projection = new DefaultKnowledgeProjector().Project(decision);

        var citationNode = Assert.Single(projection.Nodes, n => n.Id == "citation:0");
        Assert.Equal("LegalBasis", citationNode.Type);
        Assert.Equal("Law Legea 227/2015 Art. 1 (RO)", citationNode.Label);

        var edge = Assert.Single(projection.Edges);
        Assert.Equal("decision", edge.FromNodeId);
        Assert.Equal("citation:0", edge.ToNodeId);
        Assert.Equal("based-on", edge.Label);
    }

    [Fact]
    public async Task Does_not_project_a_purpose_node()
    {
        var decision = await DecisionWith(Citation("Art. 1"));

        var projection = new DefaultKnowledgeProjector().Project(decision);

        Assert.DoesNotContain(projection.Nodes, n => n.Type == "Purpose");
    }

    [Fact]
    public async Task Narrative_is_the_canonical_decision_narrative()
    {
        var decision = await DecisionWith(Citation("Art. 1"));

        var projection = new DefaultKnowledgeProjector().Project(decision);

        Assert.Equal(
            ExplanationNarrativeProjector.Project(decision.Explanation).Lines,
            projection.Narrative.Lines);
        Assert.Contains("Decision: StubCategory.", projection.Narrative.Text);
        Assert.Contains("resolved to 1 governing citation(s).", projection.Narrative.Text);
    }

    [Fact]
    public async Task Empty_legal_basis_yields_only_a_decision_node()
    {
        var eligible = new FiscalSubject { Revenue = 320_000m, EmployeeCount = 3 };
        var engine = new ClassificationEngine(
            new DefaultRuleRegistry(new ClassificationRule[] { new MicroenterpriseClassificationRule() }));
        var decision = await engine.ClassifyAsync(eligible);

        var projection = new DefaultKnowledgeProjector().Project(decision);

        var node = Assert.Single(projection.Nodes);
        Assert.Equal("decision", node.Id);
        Assert.Empty(projection.Edges);
    }

    [Fact]
    public async Task Every_edge_endpoint_resolves_to_a_node()
    {
        var decision = await DecisionWith(Citation("Art. 1"));

        var projection = new DefaultKnowledgeProjector().Project(decision);

        var nodeIds = projection.Nodes.Select(n => n.Id).ToHashSet();
        Assert.All(projection.Edges, edge =>
        {
            Assert.Contains(edge.FromNodeId, nodeIds);
            Assert.Contains(edge.ToNodeId, nodeIds);
        });
    }

    [Fact]
    public void Duplicate_node_ids_fail_fast()
    {
        var nodes = new[]
        {
            new ExplanationNode("dup", "A", "Decision"),
            new ExplanationNode("dup", "B", "LegalBasis")
        };

        Assert.Throws<InvalidOperationException>(() =>
            new KnowledgeProjectionResult(nodes, Array.Empty<ExplanationEdge>(),
                new ExplanationNarrative(Array.Empty<string>())));
    }

    [Fact]
    public void Edge_referencing_unknown_node_fails_fast()
    {
        var nodes = new[] { new ExplanationNode("decision", "A", "Decision") };
        var edges = new[] { new ExplanationEdge("decision", "ghost", "based-on") };

        Assert.Throws<InvalidOperationException>(() =>
            new KnowledgeProjectionResult(nodes, edges,
                new ExplanationNarrative(Array.Empty<string>())));
    }
}
