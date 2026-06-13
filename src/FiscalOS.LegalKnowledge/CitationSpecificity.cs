using FiscalOS.LegalCore;

namespace FiscalOS.LegalKnowledge;

public static class CitationSpecificity
{
    public static SpecificityLevel SpecificityOf(LegalCitation citation)
    {
        ArgumentNullException.ThrowIfNull(citation);

        return citation.Specificity switch
        {
            SpecificityLevel.General     => SpecificityLevel.General,
            SpecificityLevel.Specific    => SpecificityLevel.Specific,
            SpecificityLevel.Exceptional => SpecificityLevel.Exceptional,
            _ => throw new ArgumentOutOfRangeException(
                nameof(citation),
                citation.Specificity,
                null)
        };
    }
}
