using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Evaluation;

namespace FiscalOS.Runtime.Classification.Rules;

public sealed class MicroenterpriseClassificationRule : ClassificationRule
{
    private const string MicroenterpriseCategory = "Microenterprise";

    // Curated (hand-authored) governing citation for the microenterprise regime.
    // Manually maintained until a legislation ingestion pipeline exists.
    private static readonly LegalCitation GoverningCitation = new(
        LegalSourceType.FiscalCode,
        "Legea 227/2015",
        "Art. 47",
        SpecificityLevel.Specific,
        new DateOnly(2016, 1, 1),
        new JurisdictionId("RO"));

    private readonly MicroenterpriseEligibilityRule _eligibility = new();

    public string RuleId => "MICROENTERPRISE_ELIGIBILITY";

    public string Description =>
        "Classifies a fiscal subject as a microenterprise per the eligibility thresholds.";

    public int Priority => 100;

    public RuleEvaluationResult Evaluate(
        ClassificationContext context)
    {
        var evaluation = _eligibility.Evaluate(context.Subject);

        var eligible = evaluation.HasAssertion("MICROENTERPRISE_ELIGIBLE", true);

        return new RuleEvaluationResult(
            RuleId,
            Passed: eligible,
            Message: evaluation.Explanation?.Text,
            Category: eligible ? MicroenterpriseCategory : null)
        {
            Citations = eligible ? [GoverningCitation] : []
        };
    }
}
