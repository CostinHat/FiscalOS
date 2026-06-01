namespace FiscalOS.LegalKnowledge;

public sealed record ConflictResolutionResult(
    IReadOnlyList<LegalCitation> Winners)
{
    public bool IsResolved => Winners.Count == 1;

    public bool IsUnresolved => Winners.Count > 1;

    public bool IsEmpty => Winners.Count == 0;
}
