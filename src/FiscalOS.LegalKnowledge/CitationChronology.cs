namespace FiscalOS.LegalKnowledge;

public static class CitationChronology
{
    public static DateOnly EffectiveDateOf(LegalCitation citation)
    {
        ArgumentNullException.ThrowIfNull(citation);

        if (citation.EffectiveDate == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(citation),
                citation.EffectiveDate,
                null);
        }

        return citation.EffectiveDate;
    }
}
