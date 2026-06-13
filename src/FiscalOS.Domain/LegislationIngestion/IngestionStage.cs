namespace FiscalOS.Domain.LegislationIngestion;

public enum IngestionStage
{
    Discovery,
    Acquisition,
    Normalization,
    RawDocumentIdentity,
    Versioning,
    CitationDetection,
    CandidateExtraction,
    HumanReview,
    CuratedPromotion,
    RuleBinding
}
