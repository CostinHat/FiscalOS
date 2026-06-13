using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime;

// Single curated definition for the VAT-payer regime: the registration revenue
// threshold and the two governing citations. Hand-maintained until a legislation
// ingestion pipeline exists.
//
// The two citations deliberately compete: the statutory Fiscal Code provision and
// a later, lower-authority ANAF order both address the determination. Existing
// ConflictResolver (lex superior) elects the statutory citation as governing.
public static class VatRegime
{
    public const decimal RevenueThreshold = 300_000m;

    // Governing statutory basis (Statutory authority).
    public static readonly LegalCitation StatutoryCitation = new(
        LegalSourceType.FiscalCode,
        "Legea 227/2015",
        "Art. 310",
        SpecificityLevel.Specific,
        new DateOnly(2016, 1, 1),
        new JurisdictionId("RO"));

    // Competing administrative basis (Administrative authority, later in time).
    public static readonly LegalCitation AdministrativeCitation = new(
        LegalSourceType.ANAFOrder,
        "OPANAF 1699/2021",
        "Art. 1",
        SpecificityLevel.Specific,
        new DateOnly(2021, 1, 1),
        new JurisdictionId("RO"));
}
