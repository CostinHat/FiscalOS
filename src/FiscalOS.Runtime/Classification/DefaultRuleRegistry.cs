using System.Collections.Generic;
using System.Linq;

namespace FiscalOS.Runtime.Classification;

public sealed class DefaultRuleRegistry : RuleRegistry
{
    private readonly IReadOnlyCollection<ClassificationRule> _rules;

    public DefaultRuleRegistry(
        IEnumerable<ClassificationRule> rules)
    {
        _rules = rules.ToList();
    }

    public IReadOnlyCollection<ClassificationRule> GetRules()
    {
        return _rules;
    }
}