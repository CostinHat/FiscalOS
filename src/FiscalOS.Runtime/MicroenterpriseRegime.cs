using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime;

// Single curated definition binding the microenterprise eligibility thresholds
// to their governing legal citation, so the two cannot drift. Both the
// eligibility logic and the emitted citation read from here. Hand-maintained
// until a legislation ingestion pipeline exists.
public static class MicroenterpriseRegime
{
    public const decimal RevenueThreshold = 500_000m;

    public const int MinimumEmployeeCount = 1;

    public static readonly LegalCitation Citation = new(
        LegalSourceType.FiscalCode,
        "Legea 227/2015",
        "Art. 47",
        SpecificityLevel.Specific,
        new DateOnly(2016, 1, 1),
        new JurisdictionId("RO"));
}
