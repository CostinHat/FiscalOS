namespace FiscalOS.Core;

public sealed class EvaluationResult
{
    private readonly List<KnowledgeAssertion> _assertions = [];

    public EvaluationResult(IEnumerable<KnowledgeAssertion> assertions, Explanation? explanation)
    {
        _assertions.AddRange(assertions);
        Explanation = explanation;
    }

    public IReadOnlyCollection<KnowledgeAssertion> Assertions => _assertions;

    public Explanation? Explanation { get; }

    public bool HasAssertion(string conceptCode, object? value = null)
    {
        return _assertions.Any(assertion =>
            assertion.ConceptCode == conceptCode &&
            (value is null || Equals(assertion.Value, value)));
    }
}
