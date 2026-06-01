using System.Linq;

namespace FiscalOS.LegalKnowledge;

public static class LexSuperior
{
    public static LexSuperiorResult Resolve(IEnumerable<LegalCitation> citations)
    {
        ArgumentNullException.ThrowIfNull(citations);

        var ranked = citations
            .Select(citation => (citation, level: CitationAuthority.LevelOf(citation)))
            .ToList();

        if (ranked.Count == 0)
        {
            return new LexSuperiorResult(null, []);
        }

        var winningLevel = ranked.Max(entry => entry.level);

        var winners = ranked
            .Where(entry => entry.level == winningLevel)
            .Select(entry => entry.citation)
            .ToList();

        return new LexSuperiorResult(winningLevel, winners);
    }
}
