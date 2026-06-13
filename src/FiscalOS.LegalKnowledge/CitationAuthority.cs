using FiscalOS.LegalCore;

namespace FiscalOS.LegalKnowledge;

public static class CitationAuthority
{
    public static SourceAuthorityLevel LevelOf(LegalCitation citation) =>
        SourceAuthority.LevelFor(citation.SourceType);

    public static SourceHierarchy HierarchyOf(LegalCitation citation) =>
        new(LevelOf(citation));
}
