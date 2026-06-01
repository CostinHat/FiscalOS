using System.Linq;
using FiscalOS.Core;
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

    public Task<ClassificationDecision> ClassifyAsync(
        FiscalSubject subject,
        CancellationToken cancellationToken = default)
    {
        var rules = _ruleRegistry
    .GetRules()
    .OrderByDescending(rule => rule.Priority)
    .ToList();

        var evaluations = rules
    .Select(rule => rule.Evaluate(
        new ClassificationContext(
            subject)))
    .ToList();

        var winningEvaluation = evaluations
    .FirstOrDefault(evaluation => evaluation.Passed);

        var result = new ClassificationResult(
    Category: winningEvaluation?.Category ?? "Unclassified",
    Explanation: winningEvaluation is null
        ? $"Executed {rules.Count} rule(s). No winning rule."
        : $"Executed {rules.Count} rule(s). Winning rule: {winningEvaluation.RuleId}.");

        var legalBasis = DecisionLegalBasis.Resolve(
    winningEvaluation?.Citations ?? []);

        var decision = new ClassificationDecision(
    Result: result,
    WinningRuleId: winningEvaluation?.RuleId,
    RuleResults: evaluations,
    LegalBasis: legalBasis);
        return Task.FromResult(decision);
    }
}