namespace FiscalOS.Runtime.Evaluation;

public interface IEvaluationEngine
{
    Task<EvaluationResult> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default);
}