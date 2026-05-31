namespace FiscalOS.LegalKnowledge;

public sealed class LegalSource
{
    public Guid Id { get; init; }

    public LegalSourceType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Reference { get; init; } = string.Empty;

    public DateOnly EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public string Content { get; init; } = string.Empty;
}