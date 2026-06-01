namespace FiscalOS.Runtime.Evaluation;

public sealed class EvaluationEngine : IEvaluationEngine
{
    public Task<EvaluationResult> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        var result = new EvaluationResult(
            IsCompliant: true,
            RuleResults: Array.Empty<RuleEvaluationResult>());

        return Task.FromResult(result);
    }
}