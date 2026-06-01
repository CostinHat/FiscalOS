using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Classification.Rules;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Execute_Classification_Rules
{
    [Fact]
    public async Task Execute_registered_rules()
    {
        var registry = new DefaultRuleRegistry(
            new ClassificationRule[]
            {
                new AlwaysPassRule()
            });

        var engine = new ClassificationEngine(
            registry);

        var result = await engine.ClassifyAsync();

        Assert.Equal(
            "Executed 1 rule(s).",
            result.Explanation);
    }
}