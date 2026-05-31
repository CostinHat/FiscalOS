namespace FiscalOS.LegalKnowledge;

public sealed class RuleCandidate
{
    public Guid Id { get; init; }

    public Guid FragmentId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string ConditionExpression { get; init; } = string.Empty;

    public string Conclusion { get; init; } = string.Empty;
}