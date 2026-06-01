using System;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Resolve_Lex_Posterior
{
    private static LegalCitation Citation(DateOnly effectiveDate, string article) =>
        new(LegalSourceType.Law, "Legea 227/2015", article, SpecificityLevel.General, effectiveDate);

    [Fact]
    public void Later_date_eliminates_earlier_date()
    {
        var earlier = Citation(new DateOnly(2020, 1, 1), "Art. 1");
        var later = Citation(new DateOnly(2024, 1, 1), "Art. 2");

        var result = LexPosterior.Resolve(new[] { earlier, later });

        Assert.Equal(new DateOnly(2024, 1, 1), result.WinningDate);
        Assert.Equal(new[] { later }, result.Winners);
        Assert.True(result.IsUnique);
    }

    [Fact]
    public void Returns_only_the_newest_tier_from_a_mixed_set()
    {
        var oldest = Citation(new DateOnly(2018, 6, 1), "Art. 1");
        var middle = Citation(new DateOnly(2021, 3, 1), "Art. 2");
        var newest = Citation(new DateOnly(2025, 1, 1), "Art. 3");

        var result = LexPosterior.Resolve(new[] { middle, newest, oldest });

        Assert.Equal(new DateOnly(2025, 1, 1), result.WinningDate);
        Assert.Equal(new[] { newest }, result.Winners);
    }

    [Fact]
    public void Ties_at_the_newest_date_return_all_newest_citations()
    {
        var older = Citation(new DateOnly(2020, 1, 1), "Art. 1");
        var firstNewest = Citation(new DateOnly(2024, 1, 1), "Art. 2");
        var secondNewest = Citation(new DateOnly(2024, 1, 1), "Art. 3");

        var result = LexPosterior.Resolve(new[] { firstNewest, older, secondNewest });

        Assert.Equal(new DateOnly(2024, 1, 1), result.WinningDate);
        Assert.Equal(new[] { firstNewest, secondNewest }, result.Winners);
        Assert.False(result.IsUnique);
    }

    [Fact]
    public void Single_citation_is_returned_unchanged()
    {
        var only = Citation(new DateOnly(2022, 7, 1), "Art. 1");

        var result = LexPosterior.Resolve(new[] { only });

        Assert.Equal(new DateOnly(2022, 7, 1), result.WinningDate);
        Assert.Equal(new[] { only }, result.Winners);
        Assert.True(result.IsUnique);
    }

    [Fact]
    public void Empty_input_is_a_no_op()
    {
        var result = LexPosterior.Resolve(Array.Empty<LegalCitation>());

        Assert.Empty(result.Winners);
        Assert.Null(result.WinningDate);
        Assert.False(result.IsUnique);
    }

    [Fact]
    public void Preserves_input_order_among_winners()
    {
        var first = Citation(new DateOnly(2024, 1, 1), "Art. 10");
        var second = Citation(new DateOnly(2024, 1, 1), "Art. 20");
        var third = Citation(new DateOnly(2024, 1, 1), "Art. 30");

        var result = LexPosterior.Resolve(new[] { first, second, third });

        Assert.Equal(new[] { first, second, third }, result.Winners);
    }

    [Fact]
    public void Default_effective_date_fails_fast()
    {
        var valid = Citation(new DateOnly(2024, 1, 1), "Art. 1");
        var incomplete = Citation(default, "Art. 2");

        Assert.Throws<ArgumentOutOfRangeException>(
            () => LexPosterior.Resolve(new[] { valid, incomplete }));
    }

    [Fact]
    public void Null_input_throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => LexPosterior.Resolve(null!));
    }
}
