using System.Linq;

namespace FiscalOS.LegalKnowledge;

public static class LexPosterior
{
    public static LexPosteriorResult Resolve(IEnumerable<LegalCitation> citations)
    {
        ArgumentNullException.ThrowIfNull(citations);

        var ranked = citations
            .Select(citation => (citation, date: CitationChronology.EffectiveDateOf(citation)))
            .ToList();

        if (ranked.Count == 0)
        {
            return new LexPosteriorResult(null, []);
        }

        var winningDate = ranked.Max(entry => entry.date);

        var winners = ranked
            .Where(entry => entry.date == winningDate)
            .Select(entry => entry.citation)
            .ToList();

        return new LexPosteriorResult(winningDate, winners);
    }
}
