using System.Linq;
using System;
using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Resolve_Lex_Specialis
{
    private static LegalCitation Citation(SpecificityLevel specificity, string article) =>
        new(LegalSourceType.Law, "Legea 227/2015", article, specificity, new DateOnly(2020, 1, 1), new JurisdictionId("RO"));

    [Fact]
    public void More_specific_eliminates_more_general()
    {
        var general = Citation(SpecificityLevel.General, "Art. 1");
        var specific = Citation(SpecificityLevel.Specific, "Art. 2");

        var result = LexSpecialis.Resolve(new[] { general, specific });

        Assert.Equal(SpecificityLevel.Specific, result.WinningSpecificity);
        Assert.Equal(new[] { specific }, result.Winners);
        Assert.True(result.IsUnique);
    }

    [Fact]
    public void Returns_only_the_most_specific_tier_from_a_mixed_set()
    {
        var general = Citation(SpecificityLevel.General, "Art. 1");
        var specific = Citation(SpecificityLevel.Specific, "Art. 2");
        var exceptional = Citation(SpecificityLevel.Exceptional, "Art. 3");

        var result = LexSpecialis.Resolve(new[] { general, exceptional, specific });

        Assert.Equal(SpecificityLevel.Exceptional, result.WinningSpecificity);
        Assert.Equal(new[] { exceptional }, result.Winners);
    }

    [Fact]
    public void Ties_at_the_most_specific_tier_return_all_top_tier_citations()
    {
        var general = Citation(SpecificityLevel.General, "Art. 1");
        var firstSpecific = Citation(SpecificityLevel.Specific, "Art. 2");
        var secondSpecific = Citation(SpecificityLevel.Specific, "Art. 3");

        var result = LexSpecialis.Resolve(new[] { firstSpecific, general, secondSpecific });

        Assert.Equal(SpecificityLevel.Specific, result.WinningSpecificity);
        Assert.Equal(new[] { firstSpecific, secondSpecific }, result.Winners);
        Assert.False(result.IsUnique);
    }

    [Fact]
    public void Single_citation_is_returned_unchanged()
    {
        var general = Citation(SpecificityLevel.General, "Art. 1");

        var result = LexSpecialis.Resolve(new[] { general });

        Assert.Equal(SpecificityLevel.General, result.WinningSpecificity);
        Assert.Equal(new[] { general }, result.Winners);
        Assert.True(result.IsUnique);
    }

    [Fact]
    public void Empty_input_is_a_no_op()
    {
        var result = LexSpecialis.Resolve(Array.Empty<LegalCitation>());

        Assert.Empty(result.Winners);
        Assert.Null(result.WinningSpecificity);
        Assert.False(result.IsUnique);
    }

    [Fact]
    public void Preserves_input_order_among_winners()
    {
        var first = Citation(SpecificityLevel.Specific, "Art. 10");
        var second = Citation(SpecificityLevel.Specific, "Art. 20");
        var third = Citation(SpecificityLevel.Specific, "Art. 30");

        var result = LexSpecialis.Resolve(new[] { first, second, third });

        Assert.Equal(new[] { first, second, third }, result.Winners);
    }

    [Fact]
    public void Undefined_specificity_level_fails_fast()
    {
        var valid = Citation(SpecificityLevel.Specific, "Art. 1");
        var undefined = Citation((SpecificityLevel)0, "Art. 2");

        Assert.Throws<ArgumentOutOfRangeException>(
            () => LexSpecialis.Resolve(new[] { valid, undefined }));
    }

    [Fact]
    public void Null_input_throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => LexSpecialis.Resolve(null!));
    }
}
