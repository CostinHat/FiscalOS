namespace FiscalOS.LegalKnowledge;

public sealed class LegalFragment
{
    public Guid Id { get; init; }

    public Guid SourceId { get; init; }

    public string Article { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;
}