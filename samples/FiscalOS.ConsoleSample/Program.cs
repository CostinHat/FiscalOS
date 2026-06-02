using FiscalOS.Core;
using FiscalOS.LegalKnowledge;
using FiscalOS.Runtime.Classification;
using FiscalOS.Runtime.Evaluation;

// DEL-0001 console sample: manually instantiate the validated classification
// path (no DI, no database) and run one real subject through ClassifyAsync.

var registry = new DefaultRuleRegistry(
    new ClassificationRule[] { new SampleMicroenterpriseRule() });
var engine = new ClassificationEngine(registry);

var subject = new FiscalSubject
{
    Name = "Sample SRL",
    FiscalCode = new FiscalCode("RO12345678"),
    TaxIdentificationNumber = new TaxIdentificationNumber("12345678"),
    Revenue = 320_000m,
    EmployeeCount = 3
};

var decision = await engine.ClassifyAsync(subject);

Console.WriteLine(
    "This sample uses a demonstration-only rule with a hand-authored citation "
    + "because production MicroenterpriseClassificationRule does not yet receive "
    + "citations from a legislation ingestion pipeline.");
Console.WriteLine();

Console.WriteLine("== Classification ==");
Console.WriteLine($"Category: {decision.Result.Category}");
Console.WriteLine($"Winning rule: {decision.WinningRuleId ?? "(none)"}");

Console.WriteLine();
Console.WriteLine("== Governing legal basis ==");
var governingCitations = decision.Explanation.LegalBasis.GoverningCitations;
if (governingCitations.Count == 0)
{
    Console.WriteLine("(none)");
}
else
{
    foreach (var citation in governingCitations)
    {
        Console.WriteLine(
            $"- {citation.SourceType} {citation.SourceReference} {citation.Article} ({citation.Jurisdiction.Value})");
    }
}

Console.WriteLine();
Console.WriteLine("== Narrative ==");
Console.WriteLine(ExplanationNarrativeProjector.Project(decision.Explanation).Text);

Console.WriteLine();
Console.WriteLine("== Knowledge projection (nodes) ==");
var projection = new DefaultKnowledgeProjector().Project(decision);
foreach (var node in projection.Nodes)
{
    Console.WriteLine($"- [{node.Type}] {node.Id}: {node.Label}");
}

// This sample uses a demonstration-only rule with a hand-authored citation
// because production MicroenterpriseClassificationRule does not yet receive
// citations from a legislation ingestion pipeline.
internal sealed class SampleMicroenterpriseRule : ClassificationRule
{
    public string RuleId => "MICROENTERPRISE_SAMPLE";

    public string Description => "Sample rule classifying a subject as a microenterprise.";

    public int Priority => 100;

    public RuleEvaluationResult Evaluate(ClassificationContext context) =>
        new(RuleId, true, "Sample subject qualifies as a microenterprise.", "Microenterprise")
        {
            Citations = new[]
            {
                new LegalCitation(
                    LegalSourceType.FiscalCode,
                    "Legea 227/2015",
                    "Art. 47",
                    SpecificityLevel.Specific,
                    new DateOnly(2024, 1, 1),
                    new JurisdictionId("RO"))
            }
        };
}
