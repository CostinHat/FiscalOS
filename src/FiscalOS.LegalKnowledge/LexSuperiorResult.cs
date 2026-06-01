namespace FiscalOS.LegalKnowledge;

public sealed record LexSuperiorResult(
    SourceAuthorityLevel? WinningLevel,
    IReadOnlyList<LegalCitation> Winners)
{
    public bool IsUnique => Winners.Count == 1;
}
