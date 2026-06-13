using System.Linq;
using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Order_Specificity_Levels
{
    [Fact]
    public void More_specific_compares_greater()
    {
        Assert.True(SpecificityLevel.Exceptional > SpecificityLevel.Specific);
        Assert.True(SpecificityLevel.Specific > SpecificityLevel.General);
    }

    [Fact]
    public void Underlying_values_ascend_with_specificity()
    {
        Assert.Equal(1, (int)SpecificityLevel.General);
        Assert.Equal(2, (int)SpecificityLevel.Specific);
        Assert.Equal(3, (int)SpecificityLevel.Exceptional);
    }

    [Fact]
    public void Sorting_orders_from_most_general_to_most_specific()
    {
        var unordered = new[]
        {
            SpecificityLevel.Exceptional,
            SpecificityLevel.General,
            SpecificityLevel.Specific
        };

        var ordered = unordered.OrderBy(level => level).ToArray();

        Assert.Equal(
            new[]
            {
                SpecificityLevel.General,
                SpecificityLevel.Specific,
                SpecificityLevel.Exceptional
            },
            ordered);
    }
}
