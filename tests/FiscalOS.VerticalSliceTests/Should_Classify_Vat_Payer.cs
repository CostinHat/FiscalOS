using FiscalOS.Core;
using FiscalOS.Runtime;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Classify_Vat_Payer
{
    private static FiscalSubject Subject(decimal revenue) =>
        new()
        {
            Name = "Test Company",
            FiscalCode = new FiscalCode("TEST"),
            TaxIdentificationNumber = new TaxIdentificationNumber("12345678"),
            Revenue = revenue
        };

    private static async Task<ClassificationDecision> Classify(decimal revenue)
    {
        var engine = new ClassificationEngine(
            new DefaultRuleRegistry(new ClassificationRule[] { new VatPayerClassificationRule() }));
        return await engine.ClassifyAsync(Subject(revenue));
    }

    [Fact]
    public async Task Classifies_a_subject_above_the_threshold_as_vat_payer()
    {
        var decision = await Classify(400_000m);

        Assert.Equal("VatPayer", decision.Result.Category);
    }

    [Fact]
    public async Task Considers_both_competing_citations()
    {
        var decision = await Classify(400_000m);

        Assert.Equal(2, decision.Explanation.LegalBasis.ConsideredCitations.Count);
    }

    [Fact]
    public async Task Resolves_to_a_single_governing_citation()
    {
        var decision = await Classify(400_000m);

        Assert.Single(decision.Explanation.LegalBasis.GoverningCitations);
    }

    [Fact]
    public async Task Governing_citation_is_the_statutory_one()
    {
        var decision = await Classify(400_000m);

        var governing = Assert.Single(decision.Explanation.LegalBasis.GoverningCitations);
        Assert.Equal(VatRegime.StatutoryCitation, governing);
    }

    [Fact]
    public async Task Administrative_citation_is_not_governing()
    {
        var decision = await Classify(400_000m);

        Assert.DoesNotContain(VatRegime.AdministrativeCitation, decision.Explanation.LegalBasis.GoverningCitations);
    }

    [Fact]
    public async Task No_unresolved_legal_conflict()
    {
        var decision = await Classify(400_000m);

        Assert.False(decision.Explanation.HasUnresolvedLegalConflict);
    }
}
