using System.Linq;
using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Order_Source_Authority_Levels
{
    [Fact]
    public void Higher_authority_compares_greater()
    {
        Assert.True(SourceAuthorityLevel.Constitutional > SourceAuthorityLevel.Statutory);
        Assert.True(SourceAuthorityLevel.Statutory > SourceAuthorityLevel.Government);
        Assert.True(SourceAuthorityLevel.Government > SourceAuthorityLevel.Administrative);
        Assert.True(SourceAuthorityLevel.Administrative > SourceAuthorityLevel.Unknown);
    }

    [Fact]
    public void Underlying_values_ascend_with_authority()
    {
        Assert.Equal(0, (int)SourceAuthorityLevel.Unknown);
        Assert.Equal(1, (int)SourceAuthorityLevel.Administrative);
        Assert.Equal(2, (int)SourceAuthorityLevel.Government);
        Assert.Equal(3, (int)SourceAuthorityLevel.Statutory);
        Assert.Equal(4, (int)SourceAuthorityLevel.Constitutional);
    }

    [Fact]
    public void Sorting_orders_from_lowest_to_highest_authority()
    {
        var unordered = new[]
        {
            SourceAuthorityLevel.Statutory,
            SourceAuthorityLevel.Unknown,
            SourceAuthorityLevel.Constitutional,
            SourceAuthorityLevel.Administrative,
            SourceAuthorityLevel.Government
        };

        var ordered = unordered.OrderBy(level => level).ToArray();

        Assert.Equal(
            new[]
            {
                SourceAuthorityLevel.Unknown,
                SourceAuthorityLevel.Administrative,
                SourceAuthorityLevel.Government,
                SourceAuthorityLevel.Statutory,
                SourceAuthorityLevel.Constitutional
            },
            ordered);
    }
}
