namespace FiscalOS.Runtime.Evaluation;

public sealed class EvaluationEngine : IEvaluationEngine
{
    public Task<EvaluationResult> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new EvaluationResult(Array.Empty<RuleEvaluationResult>()));
    }
}