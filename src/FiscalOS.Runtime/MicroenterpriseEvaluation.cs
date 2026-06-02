using FiscalOS.Core;
using FiscalOS.LegalKnowledge;

namespace FiscalOS.Runtime;

// Result of a microenterprise eligibility evaluation, surfacing the governing
// citation produced by the same layer that applies the regime threshold.
public sealed record MicroenterpriseEvaluation(
    EvaluationResult Result,
    LegalCitation? GoverningCitation);
