using System;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Resolve_Citation_Authority
{
    private static LegalCitation Citation(LegalSourceType sourceType) =>
        new(sourceType, "Legea 227/2015", "Art. 47", SpecificityLevel.General);

    [Theory]
    [InlineData(LegalSourceType.Law, SourceAuthorityLevel.Statutory)]
    [InlineData(LegalSourceType.EmergencyOrdinance, SourceAuthorityLevel.Statutory)]
    [InlineData(LegalSourceType.FiscalCode, SourceAuthorityLevel.Statutory)]
    [InlineData(LegalSourceType.GovernmentDecision, SourceAuthorityLevel.Government)]
    [InlineData(LegalSourceType.MethodologicalNorm, SourceAuthorityLevel.Government)]
    [InlineData(LegalSourceType.ANAFOrder, SourceAuthorityLevel.Administrative)]
    [InlineData(LegalSourceType.Other, SourceAuthorityLevel.Unknown)]
    public void Resolves_citation_to_expected_hierarchy(
        LegalSourceType sourceType,
        SourceAuthorityLevel expected)
    {
        var citation = Citation(sourceType);

        Assert.Equal(expected, CitationAuthority.LevelOf(citation));
        Assert.Equal(new SourceHierarchy(expected), CitationAuthority.HierarchyOf(citation));
    }

    [Fact]
    public void Higher_authority_citation_outranks_lower_authority_citation()
    {
        var law = CitationAuthority.HierarchyOf(Citation(LegalSourceType.Law));
        var order = CitationAuthority.HierarchyOf(Citation(LegalSourceType.ANAFOrder));

        Assert.True(law.IsHigherThan(order));
        Assert.True(order.IsLowerThan(law));
    }

    [Fact]
    public void Citations_in_the_same_tier_resolve_to_equal_authority()
    {
        var law = CitationAuthority.HierarchyOf(Citation(LegalSourceType.Law));
        var fiscalCode = CitationAuthority.HierarchyOf(Citation(LegalSourceType.FiscalCode));
        var ordinance = CitationAuthority.HierarchyOf(Citation(LegalSourceType.EmergencyOrdinance));

        Assert.True(law.IsEqualTo(fiscalCode));
        Assert.True(fiscalCode.IsEqualTo(ordinance));
    }

    [Fact]
    public void Propagates_fail_fast_for_an_undefined_source_type()
    {
        var citation = Citation((LegalSourceType)999);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CitationAuthority.HierarchyOf(citation));
    }
}
