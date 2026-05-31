namespace FiscalOS.LegalKnowledge;

public sealed class RuleVersion
{
    public Guid Id { get; init; }

    public Guid RuleId { get; init; }

    public string Version { get; init; } = string.Empty;

    public DateOnly CreatedAt { get; init; }

    public string ChangeReason { get; init; } = string.Empty;
}