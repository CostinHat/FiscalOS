using System.Collections.Generic;

namespace FiscalOS.Runtime.Classification;

public interface RuleRegistry
{
    IReadOnlyCollection<ClassificationRule> GetRules();
}