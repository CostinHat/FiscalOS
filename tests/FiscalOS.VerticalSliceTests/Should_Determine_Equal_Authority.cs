using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Determine_Equal_Authority
{
    [Fact]
    public void Same_level_is_equal()
    {
        var a = new SourceHierarchy(SourceAuthorityLevel.Statutory);
        var b = new SourceHierarchy(SourceAuthorityLevel.Statutory);

        Assert.True(a.IsEqualTo(b));
    }

    [Fact]
    public void Different_levels_are_not_equal()
    {
        var statutory = new SourceHierarchy(SourceAuthorityLevel.Statutory);
        var administrative = new SourceHierarchy(SourceAuthorityLevel.Administrative);

        Assert.False(statutory.IsEqualTo(administrative));
    }

    [Fact]
    public void Equal_authority_matches_record_value_equality()
    {
        var a = new SourceHierarchy(SourceAuthorityLevel.Government);
        var b = new SourceHierarchy(SourceAuthorityLevel.Government);

        Assert.True(a.IsEqualTo(b));
        Assert.Equal(a, b);
    }
}
