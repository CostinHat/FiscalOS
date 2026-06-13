using System;
using System.Collections.Generic;
using System.Linq;
using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Map_Source_Type_To_Authority_Level
{
    private static readonly IReadOnlyDictionary<LegalSourceType, SourceAuthorityLevel> Expected =
        new Dictionary<LegalSourceType, SourceAuthorityLevel>
        {
            [LegalSourceType.Law] = SourceAuthorityLevel.Statutory,
            [LegalSourceType.EmergencyOrdinance] = SourceAuthorityLevel.Statutory,
            [LegalSourceType.FiscalCode] = SourceAuthorityLevel.Statutory,
            [LegalSourceType.GovernmentDecision] = SourceAuthorityLevel.Government,
            [LegalSourceType.MethodologicalNorm] = SourceAuthorityLevel.Government,
            [LegalSourceType.ANAFOrder] = SourceAuthorityLevel.Administrative,
            [LegalSourceType.Other] = SourceAuthorityLevel.Unknown
        };

    [Theory]
    [InlineData(LegalSourceType.Law, SourceAuthorityLevel.Statutory)]
    [InlineData(LegalSourceType.EmergencyOrdinance, SourceAuthorityLevel.Statutory)]
    [InlineData(LegalSourceType.FiscalCode, SourceAuthorityLevel.Statutory)]
    [InlineData(LegalSourceType.GovernmentDecision, SourceAuthorityLevel.Government)]
    [InlineData(LegalSourceType.MethodologicalNorm, SourceAuthorityLevel.Government)]
    [InlineData(LegalSourceType.ANAFOrder, SourceAuthorityLevel.Administrative)]
    [InlineData(LegalSourceType.Other, SourceAuthorityLevel.Unknown)]
    public void Maps_each_source_type_to_its_authority_level(
        LegalSourceType sourceType,
        SourceAuthorityLevel expected)
    {
        Assert.Equal(expected, SourceAuthority.LevelFor(sourceType));
    }

    [Fact]
    public void Every_declared_source_type_is_mapped_as_expected()
    {
        foreach (var sourceType in Enum.GetValues<LegalSourceType>())
        {
            Assert.True(
                Expected.ContainsKey(sourceType),
                $"LegalSourceType.{sourceType} has no expected mapping defined.");

            Assert.Equal(Expected[sourceType], SourceAuthority.LevelFor(sourceType));
        }
    }

    [Fact]
    public void Never_maps_a_declared_source_type_to_constitutional()
    {
        var levels = Enum.GetValues<LegalSourceType>()
            .Select(SourceAuthority.LevelFor);

        Assert.DoesNotContain(SourceAuthorityLevel.Constitutional, levels);
    }

    [Fact]
    public void Throws_for_an_undefined_source_type()
    {
        var undefined = (LegalSourceType)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => SourceAuthority.LevelFor(undefined));
    }
}
