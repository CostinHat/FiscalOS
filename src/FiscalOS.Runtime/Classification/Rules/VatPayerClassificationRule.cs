using FiscalOS.Runtime.Evaluation;

namespace FiscalOS.Runtime.Classification.Rules;

public sealed class VatPayerClassificationRule : ClassificationRule
{
    private const string VatPayerCategory = "VatPayer";

    public string RuleId => "VAT_PAYER";

    public string Description =>
        "Classifies a fiscal subject as a VAT payer per the registration revenue threshold.";

    public int Priority => 100;

    public RuleEvaluationResult Evaluate(ClassificationContext context)
    {
        var revenue = context.Subject.Revenue;

        var eligible = revenue >= VatRegime.RevenueThreshold;

        return new RuleEvaluationResult(
            RuleId,
            Passed: eligible,
            Message: eligible
                ? $"VAT_PAYER because Revenue={revenue} >= {VatRegime.RevenueThreshold}, per {VatRegime.StatutoryCitation.SourceReference} {VatRegime.StatutoryCitation.Article}."
                : $"VAT_PAYER was not produced because Revenue={revenue} < {VatRegime.RevenueThreshold}.",
            Category: eligible ? VatPayerCategory : null)
        {
            Citations = eligible
                ? [VatRegime.StatutoryCitation, VatRegime.AdministrativeCitation]
                : []
        };
    }
}
