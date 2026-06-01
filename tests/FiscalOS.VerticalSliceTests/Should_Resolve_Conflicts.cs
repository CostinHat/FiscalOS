using System;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Resolve_Conflicts
{
    private static LegalCitation Citation(
        LegalSourceType sourceType,
        SpecificityLevel specificity,
        DateOnly effectiveDate,
        string article) =>
        new(sourceType, "Legea 227/2015", article, specificity, effectiveDate);

    [Fact]
    public void Authority_dominates_specificity_and_recency()
    {
        var law = Citation(LegalSourceType.Law, SpecificityLevel.General, new DateOnly(2018, 1, 1), "Art. 1");
        var order = Citation(LegalSourceType.ANAFOrder, SpecificityLevel.Exceptional, new DateOnly(2025, 1, 1), "Art. 2");

        var result = ConflictResolver.Resolve(new[] { order, law });

        Assert.True(result.IsResolved);
        Assert.Equal(new[] { law }, result.Winners);
    }

    [Fact]
    public void More_specific_earlier_norm_beats_later_general_norm()
    {
        // lex posterior generalis non derogat legi priori speciali:
        // specialis runs before posterior, so the earlier Specific norm survives.
        var specificOld = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2018, 1, 1), "Art. 1");
        var generalNew = Citation(LegalSourceType.Law, SpecificityLevel.General, new DateOnly(2025, 1, 1), "Art. 2");

        var result = ConflictResolver.Resolve(new[] { generalNew, specificOld });

        Assert.True(result.IsResolved);
        Assert.Equal(new[] { specificOld }, result.Winners);
    }

    [Fact]
    public void Newer_wins_when_authority_and_specificity_tie()
    {
        var older = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2018, 1, 1), "Art. 1");
        var newer = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2025, 1, 1), "Art. 2");

        var result = ConflictResolver.Resolve(new[] { older, newer });

        Assert.True(result.IsResolved);
        Assert.Equal(new[] { newer }, result.Winners);
    }

    [Fact]
    public void Resolves_to_a_single_winner_through_all_three_stages()
    {
        var lawSpecific = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2025, 1, 1), "Art. 1");
        var lawGeneral = Citation(LegalSourceType.Law, SpecificityLevel.General, new DateOnly(2030, 1, 1), "Art. 2");
        var decisionExceptional = Citation(LegalSourceType.GovernmentDecision, SpecificityLevel.Exceptional, new DateOnly(2031, 1, 1), "Art. 3");

        var result = ConflictResolver.Resolve(new[] { lawGeneral, decisionExceptional, lawSpecific });

        Assert.True(result.IsResolved);
        Assert.Equal(new[] { lawSpecific }, result.Winners);
    }

    [Fact]
    public void Unresolved_tie_returns_all_survivors()
    {
        var x = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 1");
        var y = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 2");

        var result = ConflictResolver.Resolve(new[] { x, y });

        Assert.True(result.IsUnresolved);
        Assert.False(result.IsResolved);
        Assert.Equal(new[] { x, y }, result.Winners);
    }

    [Fact]
    public void Empty_input_is_empty()
    {
        var result = ConflictResolver.Resolve(Array.Empty<LegalCitation>());

        Assert.True(result.IsEmpty);
        Assert.False(result.IsResolved);
        Assert.False(result.IsUnresolved);
        Assert.Empty(result.Winners);
    }

    [Fact]
    public void Single_citation_resolves_to_itself()
    {
        var only = Citation(LegalSourceType.ANAFOrder, SpecificityLevel.General, new DateOnly(2020, 1, 1), "Art. 1");

        var result = ConflictResolver.Resolve(new[] { only });

        Assert.True(result.IsResolved);
        Assert.Equal(new[] { only }, result.Winners);
    }

    [Fact]
    public void Preserves_input_order_among_unresolved_survivors()
    {
        var first = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 10");
        var second = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 20");
        var third = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 30");

        var result = ConflictResolver.Resolve(new[] { first, second, third });

        Assert.True(result.IsUnresolved);
        Assert.Equal(new[] { first, second, third }, result.Winners);
    }

    [Fact]
    public void Default_date_fails_fast_at_the_posterior_stage()
    {
        // Both are top authority (Statutory) and top specificity present (Specific),
        // so both survive LexSuperior and LexSpecialis and reach LexPosterior,
        // where the default(DateOnly) citation fails fast.
        var valid = Citation(LegalSourceType.Law, SpecificityLevel.Specific, new DateOnly(2024, 1, 1), "Art. 1");
        var incomplete = Citation(LegalSourceType.Law, SpecificityLevel.Specific, default, "Art. 2");

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ConflictResolver.Resolve(new[] { valid, incomplete }));
    }

    [Fact]
    public void Null_input_throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => ConflictResolver.Resolve(null!));
    }
}
