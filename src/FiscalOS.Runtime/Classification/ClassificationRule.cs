using FiscalOS.Runtime.Evaluation;
namespace FiscalOS.Runtime.Classification;

public interface ClassificationRule
{
    string RuleId { get; }

    string Description { get; }

    RuleEvaluationResult Evaluate(
        ClassificationContext context
    );
}