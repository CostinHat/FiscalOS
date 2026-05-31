namespace FiscalOS.LegalKnowledge;

public sealed class FiscalRule
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public DateOnly EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public string ConditionExpression { get; init; } = string.Empty;

    public string Conclusion { get; init; } = string.Empty;

    public string LegalReference { get; init; } = string.Empty;
}