using System.Linq;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Sort_By_Authority
{
    [Fact]
    public void Sorts_from_lowest_to_highest_authority()
    {
        var unordered = new[]
        {
            new SourceHierarchy(SourceAuthorityLevel.Statutory),
            new SourceHierarchy(SourceAuthorityLevel.Unknown),
            new SourceHierarchy(SourceAuthorityLevel.Constitutional),
            new SourceHierarchy(SourceAuthorityLevel.Administrative),
            new SourceHierarchy(SourceAuthorityLevel.Government)
        };

        var ordered = unordered.OrderBy(h => h).ToArray();

        Assert.Equal(
            new[]
            {
                new SourceHierarchy(SourceAuthorityLevel.Unknown),
                new SourceHierarchy(SourceAuthorityLevel.Administrative),
                new SourceHierarchy(SourceAuthorityLevel.Government),
                new SourceHierarchy(SourceAuthorityLevel.Statutory),
                new SourceHierarchy(SourceAuthorityLevel.Constitutional)
            },
            ordered);
    }

    [Fact]
    public void Highest_authority_is_last_after_sorting()
    {
        var unordered = new[]
        {
            new SourceHierarchy(SourceAuthorityLevel.Administrative),
            new SourceHierarchy(SourceAuthorityLevel.Constitutional),
            new SourceHierarchy(SourceAuthorityLevel.Government)
        };

        var highest = unordered.OrderBy(h => h).Last();

        Assert.Equal(new SourceHierarchy(SourceAuthorityLevel.Constitutional), highest);
    }
}
