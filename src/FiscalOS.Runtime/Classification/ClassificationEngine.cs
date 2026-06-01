using FiscalOS.Core.Classification;

namespace FiscalOS.Runtime.Classification;

public sealed class ClassificationEngine
{
    private readonly RuleRegistry _ruleRegistry;

    public ClassificationEngine(
        RuleRegistry ruleRegistry)
    {
        _ruleRegistry = ruleRegistry;
    }

    public Task<ClassificationResult> ClassifyAsync(
        CancellationToken cancellationToken = default)
    {
        var rules = _ruleRegistry.GetRules();

        foreach (var rule in rules)
        {
            rule.Evaluate(
                new ClassificationContext(
                    new object()));
        }

        var result = new ClassificationResult(
            Category: "Unclassified",
            Explanation: $"Executed {rules.Count} rule(s).");

        return Task.FromResult(result);
    }
}