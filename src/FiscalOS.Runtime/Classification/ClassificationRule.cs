using FiscalOS.Runtime.Evaluation;
namespace FiscalOS.Runtime.Classification;

public interface ClassificationRule
{
    string RuleId { get; }

    string Description { get; }

    int Priority { get; }

    RuleEvaluationResult Evaluate(
        ClassificationContext context
    );
}