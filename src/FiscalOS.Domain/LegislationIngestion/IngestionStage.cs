namespace FiscalOS.Domain.LegislationIngestion;

public enum IngestionStage
{
    Discovery,
    Acquisition,
    Versioning,
    Normalization,
    CitationDetection,
    CandidateExtraction,
    HumanReview,
    CuratedPromotion,
    RuleBinding
}
