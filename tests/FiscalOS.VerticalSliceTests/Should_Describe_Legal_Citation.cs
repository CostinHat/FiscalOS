using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Legal_Citation
{
    [Fact]
    public void Exposes_its_components()
    {
        var citation = new LegalCitation(
            LegalSourceType.FiscalCode,
            "Legea 227/2015",
            "Art. 47",
            SpecificityLevel.Specific,
            new DateOnly(2024, 5, 1));

        Assert.Equal(LegalSourceType.FiscalCode, citation.SourceType);
        Assert.Equal("Legea 227/2015", citation.SourceReference);
        Assert.Equal("Art. 47", citation.Article);
        Assert.Equal(SpecificityLevel.Specific, citation.Specificity);
        Assert.Equal(new DateOnly(2024, 5, 1), citation.EffectiveDate);
    }

    [Fact]
    public void Citations_with_the_same_components_are_equal()
    {
        var a = new LegalCitation(LegalSourceType.Law, "Legea 227/2015", "Art. 47", SpecificityLevel.General, new DateOnly(2020, 1, 1));
        var b = new LegalCitation(LegalSourceType.Law, "Legea 227/2015", "Art. 47", SpecificityLevel.General, new DateOnly(2020, 1, 1));

        Assert.Equal(a, b);
    }

    [Fact]
    public void Citations_that_differ_are_not_equal()
    {
        var a = new LegalCitation(LegalSourceType.Law, "Legea 227/2015", "Art. 47", SpecificityLevel.General, new DateOnly(2020, 1, 1));
        var differentArticle = a with { Article = "Art. 48" };
        var differentType = a with { SourceType = LegalSourceType.ANAFOrder };
        var differentSpecificity = a with { Specificity = SpecificityLevel.Exceptional };
        var differentDate = a with { EffectiveDate = new DateOnly(2021, 1, 1) };

        Assert.NotEqual(a, differentArticle);
        Assert.NotEqual(a, differentType);
        Assert.NotEqual(a, differentSpecificity);
        Assert.NotEqual(a, differentDate);
    }

    [Fact]
    public void With_expression_does_not_mutate_the_original()
    {
        var original = new LegalCitation(LegalSourceType.FiscalCode, "Legea 227/2015", "Art. 47", SpecificityLevel.General, new DateOnly(2020, 1, 1));

        var updated = original with { Article = "Art. 51" };

        Assert.Equal("Art. 47", original.Article);
        Assert.Equal("Art. 51", updated.Article);
    }
}
