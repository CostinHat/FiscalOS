using System.Linq;

namespace FiscalOS.LegalKnowledge;

public static class LexSpecialis
{
    public static LexSpecialisResult Resolve(IEnumerable<LegalCitation> citations)
    {
        ArgumentNullException.ThrowIfNull(citations);

        var ranked = citations
            .Select(citation => (citation, level: CitationSpecificity.SpecificityOf(citation)))
            .ToList();

        if (ranked.Count == 0)
        {
            return new LexSpecialisResult(null, []);
        }

        var winningSpecificity = ranked.Max(entry => entry.level);

        var winners = ranked
            .Where(entry => entry.level == winningSpecificity)
            .Select(entry => entry.citation)
            .ToList();

        return new LexSpecialisResult(winningSpecificity, winners);
    }
}
