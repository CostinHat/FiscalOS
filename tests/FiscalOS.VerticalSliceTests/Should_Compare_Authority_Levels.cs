using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Compare_Authority_Levels
{
    [Fact]
    public void CompareTo_is_positive_when_higher()
    {
        var statutory = new SourceHierarchy(SourceAuthorityLevel.Statutory);
        var government = new SourceHierarchy(SourceAuthorityLevel.Government);

        Assert.True(statutory.CompareTo(government) > 0);
    }

    [Fact]
    public void CompareTo_is_negative_when_lower()
    {
        var administrative = new SourceHierarchy(SourceAuthorityLevel.Administrative);
        var constitutional = new SourceHierarchy(SourceAuthorityLevel.Constitutional);

        Assert.True(administrative.CompareTo(constitutional) < 0);
    }

    [Fact]
    public void CompareTo_is_zero_when_equal()
    {
        var a = new SourceHierarchy(SourceAuthorityLevel.Government);
        var b = new SourceHierarchy(SourceAuthorityLevel.Government);

        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_treats_null_as_lower()
    {
        var any = new SourceHierarchy(SourceAuthorityLevel.Unknown);

        Assert.True(any.CompareTo(null) > 0);
    }
}
