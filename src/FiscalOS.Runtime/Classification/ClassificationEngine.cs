using System.Linq;
using FiscalOS.Core;
using FiscalOS.Core.Classification;

namespace FiscalOS.Runtime.Classification;

public sealed class ClassificationEngine
{
    private readonly RuleRegistry _ruleRegistry;
    private readonly ILegalBasisResolver _legalBasisResolver;

    public ClassificationEngine(
        RuleRegistry ruleRegistry)
        : this(ruleRegistry, new LegalKnowledgeLegalBasisResolver())
    {
    }

    public ClassificationEngine(
        RuleRegistry ruleRegistry,
        ILegalBasisResolver legalBasisResolver)
    {
        ArgumentNullException.ThrowIfNull(ruleRegistry);
        ArgumentNullException.ThrowIfNull(legalBasisResolver);

        _ruleRegistry = ruleRegistry;
        _legalBasisResolver = legalBasisResolver;
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

        var legalBasis = _legalBasisResolver.Resolve(
            winningEvaluation?.Citations ?? []);

        var auditGraph = DecisionAuditGraphBuilder.Build(
            evaluations,
            winningEvaluation,
            result,
            legalBasis);

        var explanation = new DecisionExplanation(legalBasis, auditGraph);

        var decision = new ClassificationDecision(
            Result: result,
            WinningRuleId: winningEvaluation?.RuleId,
            RuleResults: evaluations,
            Explanation: explanation);
        return Task.FromResult(decision);
    }
}
