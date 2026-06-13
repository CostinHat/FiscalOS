namespace FiscalOS.Domain.LegislationIngestion;

public sealed record IngestionAuditEventKind
{
    public static readonly IngestionAuditEventKind BatchCreated = new("batch-created");
    public static readonly IngestionAuditEventKind BatchStarted = new("batch-started");
    public static readonly IngestionAuditEventKind ConfigurationSnapshotSelected = new("configuration-snapshot-selected");
    public static readonly IngestionAuditEventKind SourceSelected = new("source-selected");
    public static readonly IngestionAuditEventKind SourceSkipped = new("source-skipped");
    public static readonly IngestionAuditEventKind SourceMetadataSnapshotCreated = new("source-metadata-snapshot-created");
    public static readonly IngestionAuditEventKind SourceAcquisitionStarted = new("source-acquisition-started");
    public static readonly IngestionAuditEventKind CandidateDiscovered = new("candidate-discovered");
    public static readonly IngestionAuditEventKind CandidateFetched = new("candidate-fetched");
    public static readonly IngestionAuditEventKind CandidateSkipped = new("candidate-skipped");
    public static readonly IngestionAuditEventKind UriContentMetadataNormalized = new("uri-content-metadata-normalized");
    public static readonly IngestionAuditEventKind FingerprintComputed = new("fingerprint-computed");
    public static readonly IngestionAuditEventKind RawDocumentIdentityDecisionMade = new("raw-document-identity-decision-made");
    public static readonly IngestionAuditEventKind DuplicateCandidateDetected = new("duplicate-candidate-detected");
    public static readonly IngestionAuditEventKind FailureRecorded = new("failure-recorded");
    public static readonly IngestionAuditEventKind RetryScheduled = new("retry-scheduled");
    public static readonly IngestionAuditEventKind RetryAttempted = new("retry-attempted");
    public static readonly IngestionAuditEventKind BatchCompleted = new("batch-completed");
    public static readonly IngestionAuditEventKind RepositoryHandoffPackageCreated = new("repository-handoff-package-created");

    public string Value { get; }

    public IngestionAuditEventKind(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Ingestion audit event kind cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
