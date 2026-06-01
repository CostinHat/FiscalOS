using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Determine_Higher_Authority
{
    [Fact]
    public void Higher_level_is_higher_than_lower_level()
    {
        var statutory = new SourceHierarchy(SourceAuthorityLevel.Statutory);
        var government = new SourceHierarchy(SourceAuthorityLevel.Government);

        Assert.True(statutory.IsHigherThan(government));
        Assert.False(government.IsHigherThan(statutory));
    }

    [Fact]
    public void Lower_level_is_lower_than_higher_level()
    {
        var administrative = new SourceHierarchy(SourceAuthorityLevel.Administrative);
        var constitutional = new SourceHierarchy(SourceAuthorityLevel.Constitutional);

        Assert.True(administrative.IsLowerThan(constitutional));
        Assert.False(constitutional.IsLowerThan(administrative));
    }

    [Fact]
    public void Equal_levels_are_neither_higher_nor_lower()
    {
        var a = new SourceHierarchy(SourceAuthorityLevel.Government);
        var b = new SourceHierarchy(SourceAuthorityLevel.Government);

        Assert.False(a.IsHigherThan(b));
        Assert.False(a.IsLowerThan(b));
    }
}
