namespace FiscalOS.LegalKnowledge;

public static class ConflictResolver
{
    public static ConflictResolutionResult Resolve(IEnumerable<LegalCitation> citations)
    {
        ArgumentNullException.ThrowIfNull(citations);

        var afterSuperior = LexSuperior.Resolve(citations).Winners;
        var afterSpecialis = LexSpecialis.Resolve(afterSuperior).Winners;
        var afterPosterior = LexPosterior.Resolve(afterSpecialis).Winners;

        return new ConflictResolutionResult(afterPosterior);
    }
}
