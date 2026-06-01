namespace FiscalOS.LegalKnowledge;

public sealed record LexSpecialisResult(
    SpecificityLevel? WinningSpecificity,
    IReadOnlyList<LegalCitation> Winners)
{
    public bool IsUnique => Winners.Count == 1;
}
