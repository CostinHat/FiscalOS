using System;
using System.Linq;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Resolve_Lex_Superior
{
    private static LegalCitation Citation(LegalSourceType sourceType, string article) =>
        new(sourceType, "Legea 227/2015", article);

    [Fact]
    public void Higher_authority_eliminates_lower_authority()
    {
        var law = Citation(LegalSourceType.Law, "Art. 1");
        var order = Citation(LegalSourceType.ANAFOrder, "Art. 2");

        var result = LexSuperior.Resolve(new[] { order, law });

        Assert.Equal(SourceAuthorityLevel.Statutory, result.WinningLevel);
        Assert.Equal(new[] { law }, result.Winners);
        Assert.True(result.IsUnique);
    }

    [Fact]
    public void Returns_only_the_highest_tier_from_a_mixed_set()
    {
        var law = Citation(LegalSourceType.Law, "Art. 1");
        var decision = Citation(LegalSourceType.GovernmentDecision, "Art. 2");
        var order = Citation(LegalSourceType.ANAFOrder, "Art. 3");
        var other = Citation(LegalSourceType.Other, "Art. 4");

        var result = LexSuperior.Resolve(new[] { decision, order, law, other });

        Assert.Equal(SourceAuthorityLevel.Statutory, result.WinningLevel);
        Assert.Equal(new[] { law }, result.Winners);
    }

    [Fact]
    public void Ties_at_the_top_tier_return_all_top_tier_citations()
    {
        var law = Citation(LegalSourceType.Law, "Art. 1");
        var fiscalCode = Citation(LegalSourceType.FiscalCode, "Art. 2");
        var ordinance = Citation(LegalSourceType.EmergencyOrdinance, "Art. 3");
        var order = Citation(LegalSourceType.ANAFOrder, "Art. 4");

        var result = LexSuperior.Resolve(new[] { law, order, fiscalCode, ordinance });

        Assert.Equal(SourceAuthorityLevel.Statutory, result.WinningLevel);
        Assert.Equal(new[] { law, fiscalCode, ordinance }, result.Winners);
        Assert.False(result.IsUnique);
    }

    [Fact]
    public void Single_citation_is_returned_unchanged()
    {
        var order = Citation(LegalSourceType.ANAFOrder, "Art. 1");

        var result = LexSuperior.Resolve(new[] { order });

        Assert.Equal(SourceAuthorityLevel.Administrative, result.WinningLevel);
        Assert.Equal(new[] { order }, result.Winners);
        Assert.True(result.IsUnique);
    }

    [Fact]
    public void Empty_input_is_a_no_op()
    {
        var result = LexSuperior.Resolve(Array.Empty<LegalCitation>());

        Assert.Empty(result.Winners);
        Assert.Null(result.WinningLevel);
        Assert.False(result.IsUnique);
    }

    [Fact]
    public void Preserves_input_order_among_winners()
    {
        var first = Citation(LegalSourceType.FiscalCode, "Art. 10");
        var second = Citation(LegalSourceType.Law, "Art. 20");
        var third = Citation(LegalSourceType.EmergencyOrdinance, "Art. 30");

        var result = LexSuperior.Resolve(new[] { first, second, third });

        Assert.Equal(new[] { first, second, third }, result.Winners);
    }

    [Fact]
    public void Undefined_source_type_fails_fast()
    {
        var valid = Citation(LegalSourceType.Law, "Art. 1");
        var undefined = Citation((LegalSourceType)999, "Art. 2");

        Assert.Throws<ArgumentOutOfRangeException>(
            () => LexSuperior.Resolve(new[] { valid, undefined }));
    }

    [Fact]
    public void Null_input_throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => LexSuperior.Resolve(null!));
    }
}
