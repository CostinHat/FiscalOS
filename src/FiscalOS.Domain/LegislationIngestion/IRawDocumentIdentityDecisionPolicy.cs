namespace FiscalOS.Domain.LegislationIngestion;

public interface IRawDocumentIdentityDecisionPolicy
{
    RawDocumentIdentityDecision Decide(
        RawLegislationDocument candidate,
        LegislationSourceId sourceId,
        DateTimeOffset decidedAt);
}
