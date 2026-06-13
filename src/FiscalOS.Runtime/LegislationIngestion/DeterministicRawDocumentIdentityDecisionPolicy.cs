using FiscalOS.Domain.LegislationIngestion;

namespace FiscalOS.Runtime.LegislationIngestion;

public sealed class DeterministicRawDocumentIdentityDecisionPolicy : IRawDocumentIdentityDecisionPolicy
{
    public RawDocumentIdentityDecision Decide(
        RawLegislationDocument candidate,
        LegislationSourceId sourceId,
        DateTimeOffset decidedAt)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(sourceId);

        return new RawDocumentIdentityDecision(
            new RawDocumentId($"raw-document:{sourceId.Value}:{candidate.Id.Value}"),
            null,
            candidate.Id,
            sourceId,
            decidedAt,
            "Deterministic runtime identity decision.");
    }
}
