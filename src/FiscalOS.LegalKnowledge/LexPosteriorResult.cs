namespace FiscalOS.LegalKnowledge;

public sealed record LexPosteriorResult(
    DateOnly? WinningDate,
    IReadOnlyList<LegalCitation> Winners)
{
    public bool IsUnique => Winners.Count == 1;
}
