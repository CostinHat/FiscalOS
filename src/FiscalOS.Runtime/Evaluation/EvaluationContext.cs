namespace FiscalOS.Runtime.Evaluation;

public sealed record EvaluationContext(
    string Jurisdiction,
    DateOnly EvaluationDate,
    IReadOnlyDictionary<string, object?> Facts);