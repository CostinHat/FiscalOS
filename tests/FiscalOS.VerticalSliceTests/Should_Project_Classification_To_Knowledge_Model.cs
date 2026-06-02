using System.Linq;
using FiscalOS.Core.Classification;
using FiscalOS.Runtime.Classification;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Project_Classification_To_Knowledge_Model
{
    private static KnowledgeProjectionResult Project() =>
        new DefaultKnowledgeProjector().Project(
            new ClassificationResult(
                "Microenterprise",
                "Subject qualifies as a microenterprise under the governing fiscal rule."));

    [Fact]
    public void Produces_a_decision_like_atom()
    {
        var projection = Project();

        Assert.Contains(projection.Atoms, atom => atom.Type == "Decision");
    }

    [Fact]
    public void Produces_a_purpose_like_atom()
    {
        var projection = Project();

        Assert.Contains(projection.Atoms, atom => atom.Type == "Purpose");
    }

    [Fact]
    public void Links_the_atoms_with_at_least_one_relationship()
    {
        var projection = Project();

        Assert.NotEmpty(projection.Relationships);

        var link = projection.Relationships.First();
        var atomIds = projection.Atoms.Select(atom => atom.Id).ToHashSet();
        Assert.Contains(link.FromNodeId, atomIds);
        Assert.Contains(link.ToNodeId, atomIds);
    }

    [Fact]
    public void Produces_a_non_empty_narrative()
    {
        var projection = Project();

        Assert.NotEmpty(projection.Narrative.Lines);
        Assert.False(string.IsNullOrWhiteSpace(projection.Narrative.Text));
    }
}
