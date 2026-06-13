namespace FiscalOS.Domain.LegislationIngestion;

public sealed record RawDocumentIdentityDecision
{
    public RawDocumentId RawDocumentId { get; }

    public RawDocumentVersionId? RawDocumentVersionId { get; }

    public LegislationDocumentId CandidateDocumentId { get; }

    public LegislationSourceId SourceId { get; }

    public DateTimeOffset DecidedAt { get; }

    public string Reason { get; }

    public RawDocumentIdentityDecision(
        RawDocumentId rawDocumentId,
        RawDocumentVersionId? rawDocumentVersionId,
        LegislationDocumentId candidateDocumentId,
        LegislationSourceId sourceId,
        DateTimeOffset decidedAt,
        string reason)
    {
        ArgumentNullException.ThrowIfNull(rawDocumentId);
        ArgumentNullException.ThrowIfNull(candidateDocumentId);
        ArgumentNullException.ThrowIfNull(sourceId);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Raw document identity decision reason cannot be empty.", nameof(reason));
        }

        RawDocumentId = rawDocumentId;
        RawDocumentVersionId = rawDocumentVersionId;
        CandidateDocumentId = candidateDocumentId;
        SourceId = sourceId;
        DecidedAt = decidedAt;
        Reason = reason.Trim();
    }
}
