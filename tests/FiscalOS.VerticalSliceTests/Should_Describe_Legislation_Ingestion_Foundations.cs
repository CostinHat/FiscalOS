using System;
using FiscalOS.Domain.LegislationIngestion;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Legislation_Ingestion_Foundations
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ingestion_batch_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new IngestionBatchId(value));
    }

    [Fact]
    public void Ingestion_batch_id_trims_and_exposes_value()
    {
        var id = new IngestionBatchId("  BATCH-1 ");

        Assert.Equal("BATCH-1", id.Value);
        Assert.Equal("BATCH-1", id.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Legislation_document_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new LegislationDocumentId(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Legislation_source_reference_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new LegislationSourceReference(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Legislation_source_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new LegislationSourceId(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Raw_document_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new RawDocumentId(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Raw_document_version_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new RawDocumentVersionId(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Source_document_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new SourceDocumentId(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Raw_document_content_hash_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new RawDocumentContentHash(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Raw_document_hash_algorithm_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new RawDocumentHashAlgorithm(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Source_metadata_snapshot_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new SourceMetadataSnapshotId(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Configuration_snapshot_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new ConfigurationSnapshotId(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ingestion_provenance_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new IngestionProvenanceId(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ingestion_audit_event_id_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new IngestionAuditEventId(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ingestion_audit_event_kind_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new IngestionAuditEventKind(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ingestion_audit_event_outcome_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new IngestionAuditEventOutcome(value));
    }

    [Fact]
    public void Legislation_source_id_trims_and_exposes_value()
    {
        var id = new LegislationSourceId("  monitorul-oficial ");

        Assert.Equal("monitorul-oficial", id.Value);
        Assert.Equal("monitorul-oficial", id.ToString());
    }

    [Fact]
    public void Raw_document_id_trims_and_exposes_value()
    {
        var id = new RawDocumentId("  RAW-DOC-1 ");

        Assert.Equal("RAW-DOC-1", id.Value);
        Assert.Equal("RAW-DOC-1", id.ToString());
    }

    [Fact]
    public void Raw_document_version_id_trims_and_exposes_value()
    {
        var id = new RawDocumentVersionId("  RAW-DOC-1-V1 ");

        Assert.Equal("RAW-DOC-1-V1", id.Value);
        Assert.Equal("RAW-DOC-1-V1", id.ToString());
    }

    [Fact]
    public void Source_document_id_trims_and_exposes_value()
    {
        var id = new SourceDocumentId("  MO-1-2026 ");

        Assert.Equal("MO-1-2026", id.Value);
        Assert.Equal("MO-1-2026", id.ToString());
    }

    [Fact]
    public void Raw_document_content_hash_trims_and_exposes_value()
    {
        var hash = new RawDocumentContentHash("  abc123 ");

        Assert.Equal("abc123", hash.Value);
        Assert.Equal("abc123", hash.ToString());
    }

    [Fact]
    public void Raw_document_hash_algorithm_trims_and_exposes_value()
    {
        var algorithm = new RawDocumentHashAlgorithm("  sha256 ");

        Assert.Equal("sha256", algorithm.Value);
        Assert.Equal("sha256", algorithm.ToString());
    }

    [Fact]
    public void Source_metadata_snapshot_id_trims_and_exposes_value()
    {
        var id = new SourceMetadataSnapshotId("  SRC-SNAPSHOT-1 ");

        Assert.Equal("SRC-SNAPSHOT-1", id.Value);
        Assert.Equal("SRC-SNAPSHOT-1", id.ToString());
    }

    [Fact]
    public void Configuration_snapshot_id_trims_and_exposes_value()
    {
        var id = new ConfigurationSnapshotId("  CONFIG-SNAPSHOT-1 ");

        Assert.Equal("CONFIG-SNAPSHOT-1", id.Value);
        Assert.Equal("CONFIG-SNAPSHOT-1", id.ToString());
    }

    [Fact]
    public void Ingestion_provenance_id_trims_and_exposes_value()
    {
        var id = new IngestionProvenanceId("  PROV-1 ");

        Assert.Equal("PROV-1", id.Value);
        Assert.Equal("PROV-1", id.ToString());
    }

    [Fact]
    public void Ingestion_audit_event_id_trims_and_exposes_value()
    {
        var id = new IngestionAuditEventId("  AUDIT-EVENT-1 ");

        Assert.Equal("AUDIT-EVENT-1", id.Value);
        Assert.Equal("AUDIT-EVENT-1", id.ToString());
    }

    [Fact]
    public void Ingestion_audit_event_kind_trims_and_exposes_value()
    {
        var kind = new IngestionAuditEventKind("  source-selected ");

        Assert.Equal("source-selected", kind.Value);
        Assert.Equal("source-selected", kind.ToString());
    }

    [Fact]
    public void Ingestion_audit_event_outcome_trims_and_exposes_value()
    {
        var outcome = new IngestionAuditEventOutcome("  completed ");

        Assert.Equal("completed", outcome.Value);
        Assert.Equal("completed", outcome.ToString());
    }

    [Fact]
    public void Ingestion_audit_event_kind_preserves_record_value_semantics()
    {
        var first = new IngestionAuditEventKind("source-selected");
        var second = new IngestionAuditEventKind("source-selected");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Ingestion_audit_event_outcome_preserves_record_value_semantics()
    {
        var first = new IngestionAuditEventOutcome("completed");
        var second = new IngestionAuditEventOutcome("completed");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Ingestion_audit_event_kind_exposes_documented_ingestion_event_kinds()
    {
        Assert.Equal("batch-created", IngestionAuditEventKind.BatchCreated.Value);
        Assert.Equal("batch-started", IngestionAuditEventKind.BatchStarted.Value);
        Assert.Equal("configuration-snapshot-selected", IngestionAuditEventKind.ConfigurationSnapshotSelected.Value);
        Assert.Equal("source-selected", IngestionAuditEventKind.SourceSelected.Value);
        Assert.Equal("source-skipped", IngestionAuditEventKind.SourceSkipped.Value);
        Assert.Equal("source-metadata-snapshot-created", IngestionAuditEventKind.SourceMetadataSnapshotCreated.Value);
        Assert.Equal("source-acquisition-started", IngestionAuditEventKind.SourceAcquisitionStarted.Value);
        Assert.Equal("candidate-discovered", IngestionAuditEventKind.CandidateDiscovered.Value);
        Assert.Equal("candidate-fetched", IngestionAuditEventKind.CandidateFetched.Value);
        Assert.Equal("candidate-skipped", IngestionAuditEventKind.CandidateSkipped.Value);
        Assert.Equal("uri-content-metadata-normalized", IngestionAuditEventKind.UriContentMetadataNormalized.Value);
        Assert.Equal("fingerprint-computed", IngestionAuditEventKind.FingerprintComputed.Value);
        Assert.Equal("raw-document-identity-decision-made", IngestionAuditEventKind.RawDocumentIdentityDecisionMade.Value);
        Assert.Equal("duplicate-candidate-detected", IngestionAuditEventKind.DuplicateCandidateDetected.Value);
        Assert.Equal("failure-recorded", IngestionAuditEventKind.FailureRecorded.Value);
        Assert.Equal("retry-scheduled", IngestionAuditEventKind.RetryScheduled.Value);
        Assert.Equal("retry-attempted", IngestionAuditEventKind.RetryAttempted.Value);
        Assert.Equal("batch-completed", IngestionAuditEventKind.BatchCompleted.Value);
        Assert.Equal("repository-handoff-package-created", IngestionAuditEventKind.RepositoryHandoffPackageCreated.Value);
    }

    [Fact]
    public void Ingestion_audit_event_outcome_exposes_named_outcomes()
    {
        Assert.Equal("completed", IngestionAuditEventOutcome.Completed.Value);
        Assert.Equal("skipped", IngestionAuditEventOutcome.Skipped.Value);
        Assert.Equal("failed", IngestionAuditEventOutcome.Failed.Value);
        Assert.Equal("deferred", IngestionAuditEventOutcome.Deferred.Value);
    }

    [Fact]
    public void Ingestion_audit_event_kind_is_distinct_from_ingestion_status()
    {
        var kind = IngestionAuditEventKind.SourceSelected;
        var status = IngestionStatus.Succeeded;

        Assert.NotEqual(status.ToString(), kind.Value);
    }

    [Fact]
    public void Ingestion_audit_event_outcome_is_distinct_from_ingestion_status()
    {
        var outcome = IngestionAuditEventOutcome.Completed;
        var status = IngestionStatus.Succeeded;

        Assert.NotEqual(status.ToString(), outcome.Value);
    }

    [Fact]
    public void Ingestion_audit_event_kind_is_distinct_from_trace_entries()
    {
        var timestamp = new DateTimeOffset(2026, 6, 13, 14, 0, 0, TimeSpan.Zero);
        var kind = IngestionAuditEventKind.SourceSelected;
        var traceEntry = new IngestionTraceEntry(
            IngestionStage.Acquisition,
            IngestionStatus.Succeeded,
            timestamp,
            "source selected");

        Assert.Equal("source-selected", kind.Value);
        Assert.Equal("source selected", traceEntry.Description);
        Assert.NotEqual(traceEntry.Description, kind.Value);
        Assert.NotEqual(traceEntry.GetType(), kind.GetType());
    }

    [Fact]
    public void Ingestion_audit_event_outcome_is_distinct_from_trace_entries()
    {
        var timestamp = new DateTimeOffset(2026, 6, 13, 14, 30, 0, TimeSpan.Zero);
        var outcome = IngestionAuditEventOutcome.Completed;
        var traceEntry = new IngestionTraceEntry(
            IngestionStage.Acquisition,
            IngestionStatus.Succeeded,
            timestamp,
            "completed acquisition");

        Assert.Equal("completed", outcome.Value);
        Assert.Equal(IngestionStatus.Succeeded, traceEntry.Status);
        Assert.NotEqual(traceEntry.Status.ToString(), outcome.Value);
        Assert.NotEqual(traceEntry.GetType(), outcome.GetType());
    }

    [Fact]
    public void Ingestion_provenance_and_audit_event_ids_preserve_record_value_semantics()
    {
        var firstProvenanceId = new IngestionProvenanceId("PROV-1");
        var secondProvenanceId = new IngestionProvenanceId("PROV-1");
        var firstAuditEventId = new IngestionAuditEventId("AUDIT-EVENT-1");
        var secondAuditEventId = new IngestionAuditEventId("AUDIT-EVENT-1");

        Assert.Equal(firstProvenanceId, secondProvenanceId);
        Assert.Equal(firstAuditEventId, secondAuditEventId);
    }

    [Fact]
    public void Ingestion_provenance_and_audit_event_ids_are_distinct_from_batch_identity()
    {
        var provenanceId = new IngestionProvenanceId("PROV-1");
        var auditEventId = new IngestionAuditEventId("AUDIT-EVENT-1");
        var batchId = new IngestionBatchId("BATCH-1");

        Assert.NotEqual(batchId.Value, provenanceId.Value);
        Assert.NotEqual(batchId.Value, auditEventId.Value);
    }

    [Fact]
    public void Ingestion_provenance_and_audit_event_ids_are_distinct_from_source_identity()
    {
        var provenanceId = new IngestionProvenanceId("PROV-1");
        var auditEventId = new IngestionAuditEventId("AUDIT-EVENT-1");
        var sourceId = new LegislationSourceId("monitorul-oficial");

        Assert.NotEqual(sourceId.Value, provenanceId.Value);
        Assert.NotEqual(sourceId.Value, auditEventId.Value);
    }

    [Fact]
    public void Ingestion_provenance_and_audit_event_ids_are_distinct_from_raw_document_identity()
    {
        var provenanceId = new IngestionProvenanceId("PROV-1");
        var auditEventId = new IngestionAuditEventId("AUDIT-EVENT-1");
        var rawDocumentId = new RawDocumentId("RAW-DOC-1");

        Assert.NotEqual(rawDocumentId.Value, provenanceId.Value);
        Assert.NotEqual(rawDocumentId.Value, auditEventId.Value);
    }

    [Fact]
    public void Ingestion_provenance_and_audit_event_ids_are_distinct_from_snapshot_identities()
    {
        var provenanceId = new IngestionProvenanceId("PROV-1");
        var auditEventId = new IngestionAuditEventId("AUDIT-EVENT-1");
        var sourceMetadataSnapshotId = new SourceMetadataSnapshotId("SRC-SNAPSHOT-1");
        var configurationSnapshotId = new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1");

        Assert.NotEqual(sourceMetadataSnapshotId.Value, provenanceId.Value);
        Assert.NotEqual(sourceMetadataSnapshotId.Value, auditEventId.Value);
        Assert.NotEqual(configurationSnapshotId.Value, provenanceId.Value);
        Assert.NotEqual(configurationSnapshotId.Value, auditEventId.Value);
    }

    [Fact]
    public void Ingestion_provenance_and_audit_event_ids_are_distinct_from_legal_reference_resolution_terms()
    {
        var provenanceId = new IngestionProvenanceId("PROV-1");
        var auditEventId = new IngestionAuditEventId("AUDIT-EVENT-1");
        var legalReference = new LegalReference(new[] { new ReferenceSegment("Article", "47") });
        var resolutionDecision = new ResolutionDecision(ResolutionStatus.Unresolved);

        Assert.NotEqual(legalReference.ToString(), provenanceId.Value);
        Assert.NotEqual(legalReference.ToString(), auditEventId.Value);
        Assert.NotEqual(resolutionDecision.Status.ToString(), provenanceId.Value);
        Assert.NotEqual(resolutionDecision.Status.ToString(), auditEventId.Value);
    }

    [Fact]
    public void Ingestion_provenance_record_rejects_missing_id()
    {
        Assert.Throws<ArgumentNullException>(() => new IngestionProvenanceRecord(
            null!,
            DateTimeOffset.UnixEpoch,
            new IngestionBatchId("BATCH-1"),
            null,
            null,
            null,
            null,
            "source discovered"));
    }

    [Fact]
    public void Ingestion_audit_event_record_rejects_missing_id()
    {
        Assert.Throws<ArgumentNullException>(() => new IngestionAuditEventRecord(
            null!,
            DateTimeOffset.UnixEpoch,
            new IngestionBatchId("BATCH-1"),
            IngestionAuditEventKind.SourceSelected,
            IngestionAuditEventOutcome.Completed,
            null,
            null,
            null,
            null,
            "source selected"));
    }

    [Fact]
    public void Ingestion_provenance_record_rejects_missing_batch_id()
    {
        Assert.Throws<ArgumentNullException>(() => new IngestionProvenanceRecord(
            new IngestionProvenanceId("PROV-1"),
            DateTimeOffset.UnixEpoch,
            null!,
            null,
            null,
            null,
            null,
            "source discovered"));
    }

    [Fact]
    public void Ingestion_audit_event_record_rejects_missing_batch_id()
    {
        Assert.Throws<ArgumentNullException>(() => new IngestionAuditEventRecord(
            new IngestionAuditEventId("AUDIT-EVENT-1"),
            DateTimeOffset.UnixEpoch,
            null!,
            IngestionAuditEventKind.SourceSelected,
            IngestionAuditEventOutcome.Completed,
            null,
            null,
            null,
            null,
            "source selected"));
    }

    [Fact]
    public void Ingestion_audit_event_record_rejects_missing_kind()
    {
        Assert.Throws<ArgumentNullException>(() => new IngestionAuditEventRecord(
            new IngestionAuditEventId("AUDIT-EVENT-1"),
            DateTimeOffset.UnixEpoch,
            new IngestionBatchId("BATCH-1"),
            null!,
            IngestionAuditEventOutcome.Completed,
            null,
            null,
            null,
            null,
            "source selected"));
    }

    [Fact]
    public void Ingestion_audit_event_record_rejects_missing_outcome()
    {
        Assert.Throws<ArgumentNullException>(() => new IngestionAuditEventRecord(
            new IngestionAuditEventId("AUDIT-EVENT-1"),
            DateTimeOffset.UnixEpoch,
            new IngestionBatchId("BATCH-1"),
            IngestionAuditEventKind.SourceSelected,
            null!,
            null,
            null,
            null,
            null,
            "source selected"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ingestion_provenance_record_rejects_empty_description(string description)
    {
        Assert.Throws<ArgumentException>(() => new IngestionProvenanceRecord(
            new IngestionProvenanceId("PROV-1"),
            DateTimeOffset.UnixEpoch,
            new IngestionBatchId("BATCH-1"),
            null,
            null,
            null,
            null,
            description));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ingestion_audit_event_record_rejects_empty_details(string details)
    {
        Assert.Throws<ArgumentException>(() => new IngestionAuditEventRecord(
            new IngestionAuditEventId("AUDIT-EVENT-1"),
            DateTimeOffset.UnixEpoch,
            new IngestionBatchId("BATCH-1"),
            IngestionAuditEventKind.SourceSelected,
            IngestionAuditEventOutcome.Completed,
            null,
            null,
            null,
            null,
            details));
    }

    [Fact]
    public void Ingestion_provenance_record_preserves_timestamp_and_optional_linkages()
    {
        var createdAt = new DateTimeOffset(2026, 6, 13, 12, 0, 0, TimeSpan.Zero);
        var id = new IngestionProvenanceId("PROV-1");
        var batchId = new IngestionBatchId("BATCH-1");
        var sourceId = new LegislationSourceId("monitorul-oficial");
        var rawDocumentId = new RawDocumentId("RAW-DOC-1");
        var sourceMetadataSnapshotId = new SourceMetadataSnapshotId("SRC-SNAPSHOT-1");
        var configurationSnapshotId = new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1");

        var record = new IngestionProvenanceRecord(
            id,
            createdAt,
            batchId,
            sourceId,
            rawDocumentId,
            sourceMetadataSnapshotId,
            configurationSnapshotId,
            " source acquired ");

        Assert.Same(id, record.Id);
        Assert.Equal(createdAt, record.CreatedAt);
        Assert.Same(batchId, record.BatchId);
        Assert.Same(sourceId, record.SourceId);
        Assert.Same(rawDocumentId, record.RawDocumentId);
        Assert.Same(sourceMetadataSnapshotId, record.SourceMetadataSnapshotId);
        Assert.Same(configurationSnapshotId, record.ConfigurationSnapshotId);
        Assert.Equal("source acquired", record.Description);
    }

    [Fact]
    public void Ingestion_audit_event_record_preserves_timestamp_and_optional_linkages()
    {
        var createdAt = new DateTimeOffset(2026, 6, 13, 12, 30, 0, TimeSpan.Zero);
        var id = new IngestionAuditEventId("AUDIT-EVENT-1");
        var batchId = new IngestionBatchId("BATCH-1");
        var kind = IngestionAuditEventKind.SourceSelected;
        var outcome = IngestionAuditEventOutcome.Completed;
        var sourceId = new LegislationSourceId("monitorul-oficial");
        var rawDocumentId = new RawDocumentId("RAW-DOC-1");
        var sourceMetadataSnapshotId = new SourceMetadataSnapshotId("SRC-SNAPSHOT-1");
        var configurationSnapshotId = new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1");

        var record = new IngestionAuditEventRecord(
            id,
            createdAt,
            batchId,
            kind,
            outcome,
            sourceId,
            rawDocumentId,
            sourceMetadataSnapshotId,
            configurationSnapshotId,
            " source selected ");

        Assert.Same(id, record.Id);
        Assert.Equal(createdAt, record.CreatedAt);
        Assert.Same(batchId, record.BatchId);
        Assert.Same(kind, record.Kind);
        Assert.Same(outcome, record.Outcome);
        Assert.Same(sourceId, record.SourceId);
        Assert.Same(rawDocumentId, record.RawDocumentId);
        Assert.Same(sourceMetadataSnapshotId, record.SourceMetadataSnapshotId);
        Assert.Same(configurationSnapshotId, record.ConfigurationSnapshotId);
        Assert.Equal("source selected", record.Details);
    }

    [Fact]
    public void Ingestion_provenance_and_audit_event_records_allow_missing_optional_linkages()
    {
        var provenanceRecord = new IngestionProvenanceRecord(
            new IngestionProvenanceId("PROV-1"),
            DateTimeOffset.UnixEpoch,
            new IngestionBatchId("BATCH-1"),
            null,
            null,
            null,
            null,
            "batch created");
        var auditEventRecord = new IngestionAuditEventRecord(
            new IngestionAuditEventId("AUDIT-EVENT-1"),
            DateTimeOffset.UnixEpoch,
            new IngestionBatchId("BATCH-1"),
            IngestionAuditEventKind.BatchCreated,
            IngestionAuditEventOutcome.Completed,
            null,
            null,
            null,
            null,
            "batch created");

        Assert.Null(provenanceRecord.SourceId);
        Assert.Null(provenanceRecord.RawDocumentId);
        Assert.Null(provenanceRecord.SourceMetadataSnapshotId);
        Assert.Null(provenanceRecord.ConfigurationSnapshotId);
        Assert.Equal(IngestionAuditEventKind.BatchCreated, auditEventRecord.Kind);
        Assert.Equal(IngestionAuditEventOutcome.Completed, auditEventRecord.Outcome);
        Assert.Null(auditEventRecord.SourceId);
        Assert.Null(auditEventRecord.RawDocumentId);
        Assert.Null(auditEventRecord.SourceMetadataSnapshotId);
        Assert.Null(auditEventRecord.ConfigurationSnapshotId);
    }

    [Fact]
    public void Ingestion_provenance_and_audit_event_records_preserve_record_value_semantics()
    {
        var createdAt = DateTimeOffset.UnixEpoch;
        var firstProvenanceRecord = new IngestionProvenanceRecord(
            new IngestionProvenanceId("PROV-1"),
            createdAt,
            new IngestionBatchId("BATCH-1"),
            new LegislationSourceId("monitorul-oficial"),
            new RawDocumentId("RAW-DOC-1"),
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1"),
            "source acquired");
        var secondProvenanceRecord = new IngestionProvenanceRecord(
            new IngestionProvenanceId("PROV-1"),
            createdAt,
            new IngestionBatchId("BATCH-1"),
            new LegislationSourceId("monitorul-oficial"),
            new RawDocumentId("RAW-DOC-1"),
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1"),
            "source acquired");
        var firstAuditEventRecord = new IngestionAuditEventRecord(
            new IngestionAuditEventId("AUDIT-EVENT-1"),
            createdAt,
            new IngestionBatchId("BATCH-1"),
            IngestionAuditEventKind.SourceSelected,
            IngestionAuditEventOutcome.Completed,
            new LegislationSourceId("monitorul-oficial"),
            new RawDocumentId("RAW-DOC-1"),
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1"),
            "source selected");
        var secondAuditEventRecord = new IngestionAuditEventRecord(
            new IngestionAuditEventId("AUDIT-EVENT-1"),
            createdAt,
            new IngestionBatchId("BATCH-1"),
            IngestionAuditEventKind.SourceSelected,
            IngestionAuditEventOutcome.Completed,
            new LegislationSourceId("monitorul-oficial"),
            new RawDocumentId("RAW-DOC-1"),
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1"),
            "source selected");

        Assert.Equal(firstProvenanceRecord, secondProvenanceRecord);
        Assert.Equal(firstAuditEventRecord, secondAuditEventRecord);
    }

    [Fact]
    public void Ingestion_provenance_and_audit_event_records_are_distinct_from_trace_entries()
    {
        var timestamp = new DateTimeOffset(2026, 6, 13, 13, 0, 0, TimeSpan.Zero);
        var provenanceRecord = new IngestionProvenanceRecord(
            new IngestionProvenanceId("PROV-1"),
            timestamp,
            new IngestionBatchId("BATCH-1"),
            null,
            null,
            null,
            null,
            "source acquired");
        var auditEventRecord = new IngestionAuditEventRecord(
            new IngestionAuditEventId("AUDIT-EVENT-1"),
            timestamp,
            new IngestionBatchId("BATCH-1"),
            IngestionAuditEventKind.SourceSelected,
            IngestionAuditEventOutcome.Completed,
            null,
            null,
            null,
            null,
            "source selected");
        var traceEntry = new IngestionTraceEntry(
            IngestionStage.Acquisition,
            IngestionStatus.Succeeded,
            timestamp,
            "source acquired");

        Assert.Equal(timestamp, traceEntry.Timestamp);
        Assert.Equal(timestamp, provenanceRecord.CreatedAt);
        Assert.Equal(timestamp, auditEventRecord.CreatedAt);
        Assert.NotEqual(traceEntry.Description, auditEventRecord.Details);
        Assert.NotEqual(traceEntry.GetType(), provenanceRecord.GetType());
        Assert.NotEqual(traceEntry.GetType(), auditEventRecord.GetType());
    }

    [Fact]
    public void Configuration_snapshot_preserves_created_timestamp_and_schema_version()
    {
        var createdAt = new DateTimeOffset(2026, 6, 13, 11, 0, 0, TimeSpan.Zero);

        var snapshot = new ConfigurationSnapshot(
            new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1"),
            createdAt,
            "1");

        Assert.Equal("CONFIG-SNAPSHOT-1", snapshot.Id.Value);
        Assert.Equal(createdAt, snapshot.CreatedAt);
        Assert.Equal("1", snapshot.SchemaVersion);
    }

    [Fact]
    public void Configuration_snapshot_schema_version_is_trimmed()
    {
        var snapshot = new ConfigurationSnapshot(
            new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1"),
            DateTimeOffset.UnixEpoch,
            " 1 ");

        Assert.Equal("1", snapshot.SchemaVersion);
    }

    [Fact]
    public void Configuration_snapshot_rejects_missing_id()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfigurationSnapshot(
            null!,
            DateTimeOffset.UnixEpoch,
            "1"));
    }

    [Fact]
    public void Configuration_snapshot_rejects_empty_schema_version()
    {
        Assert.Throws<ArgumentException>(() => new ConfigurationSnapshot(
            new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1"),
            DateTimeOffset.UnixEpoch,
            " "));
    }

    [Fact]
    public void Configuration_snapshot_preserves_record_value_semantics()
    {
        var createdAt = DateTimeOffset.UnixEpoch;
        var first = new ConfigurationSnapshot(
            new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1"),
            createdAt,
            "1");
        var second = new ConfigurationSnapshot(
            new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1"),
            createdAt,
            "1");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Configuration_snapshot_id_is_distinct_from_source_metadata_snapshot_id()
    {
        var configurationSnapshotId = new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1");
        var sourceMetadataSnapshotId = new SourceMetadataSnapshotId("SRC-SNAPSHOT-1");

        Assert.NotEqual(sourceMetadataSnapshotId.Value, configurationSnapshotId.Value);
    }

    [Fact]
    public void Configuration_snapshot_id_is_distinct_from_batch_identity()
    {
        var configurationSnapshotId = new ConfigurationSnapshotId("CONFIG-SNAPSHOT-1");
        var batchId = new IngestionBatchId("BATCH-1");

        Assert.NotEqual(batchId.Value, configurationSnapshotId.Value);
    }

    [Fact]
    public void Source_metadata_snapshot_preserves_metadata_and_source_linkage()
    {
        var createdAt = new DateTimeOffset(2026, 6, 13, 10, 0, 0, TimeSpan.Zero);
        var sourceId = new LegislationSourceId("monitorul-oficial");
        var metadata = new LegislationSourceMetadata(
            sourceId,
            "Monitorul Oficial",
            "official-publication");

        var snapshot = new SourceMetadataSnapshot(
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            metadata,
            createdAt,
            "1");

        Assert.Equal("SRC-SNAPSHOT-1", snapshot.Id.Value);
        Assert.Same(sourceId, snapshot.SourceId);
        Assert.Same(metadata, snapshot.Metadata);
        Assert.Equal(createdAt, snapshot.CreatedAt);
        Assert.Equal("1", snapshot.SchemaVersion);
    }

    [Fact]
    public void Source_metadata_snapshot_schema_version_is_trimmed()
    {
        var snapshot = new SourceMetadataSnapshot(
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            new LegislationSourceMetadata(
                new LegislationSourceId("monitorul-oficial"),
                "Monitorul Oficial",
                "official-publication"),
            DateTimeOffset.UnixEpoch,
            " 1 ");

        Assert.Equal("1", snapshot.SchemaVersion);
    }

    [Fact]
    public void Source_metadata_snapshot_rejects_missing_id()
    {
        Assert.Throws<ArgumentNullException>(() => new SourceMetadataSnapshot(
            null!,
            new LegislationSourceMetadata(
                new LegislationSourceId("monitorul-oficial"),
                "Monitorul Oficial",
                "official-publication"),
            DateTimeOffset.UnixEpoch,
            "1"));
    }

    [Fact]
    public void Source_metadata_snapshot_rejects_missing_metadata()
    {
        Assert.Throws<ArgumentNullException>(() => new SourceMetadataSnapshot(
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            null!,
            DateTimeOffset.UnixEpoch,
            "1"));
    }

    [Fact]
    public void Source_metadata_snapshot_rejects_empty_schema_version()
    {
        Assert.Throws<ArgumentException>(() => new SourceMetadataSnapshot(
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            new LegislationSourceMetadata(
                new LegislationSourceId("monitorul-oficial"),
                "Monitorul Oficial",
                "official-publication"),
            DateTimeOffset.UnixEpoch,
            " "));
    }

    [Fact]
    public void Source_metadata_snapshot_id_is_distinct_from_source_id()
    {
        var sourceId = new LegislationSourceId("monitorul-oficial");
        var snapshotId = new SourceMetadataSnapshotId("SRC-SNAPSHOT-1");

        Assert.NotEqual(sourceId.Value, snapshotId.Value);
    }

    [Fact]
    public void Source_metadata_snapshot_preserves_record_value_semantics()
    {
        var createdAt = DateTimeOffset.UnixEpoch;
        var metadata = new LegislationSourceMetadata(
            new LegislationSourceId("monitorul-oficial"),
            "Monitorul Oficial",
            "official-publication");
        var first = new SourceMetadataSnapshot(
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            metadata,
            createdAt,
            "1");
        var second = new SourceMetadataSnapshot(
            new SourceMetadataSnapshotId("SRC-SNAPSHOT-1"),
            metadata,
            createdAt,
            "1");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Raw_document_fingerprint_stores_hash_and_algorithm()
    {
        var hash = new RawDocumentContentHash("abc123");
        var algorithm = new RawDocumentHashAlgorithm("sha256");

        var fingerprint = new RawDocumentFingerprint(hash, algorithm);

        Assert.Same(hash, fingerprint.ContentHash);
        Assert.Same(algorithm, fingerprint.Algorithm);
    }

    [Fact]
    public void Raw_document_fingerprint_rejects_missing_hash()
    {
        Assert.Throws<ArgumentNullException>(() => new RawDocumentFingerprint(
            null!,
            new RawDocumentHashAlgorithm("sha256")));
    }

    [Fact]
    public void Raw_document_fingerprint_rejects_missing_algorithm()
    {
        Assert.Throws<ArgumentNullException>(() => new RawDocumentFingerprint(
            new RawDocumentContentHash("abc123"),
            null!));
    }

    [Fact]
    public void Raw_document_identity_terms_are_distinct_from_source_terms()
    {
        var rawDocumentId = new RawDocumentId("RAW-DOC-1");
        var rawDocumentVersionId = new RawDocumentVersionId("RAW-DOC-1-V1");
        var sourceDocumentId = new SourceDocumentId("MO-1-2026");
        var sourceId = new LegislationSourceId("monitorul-oficial");
        var sourceReference = new LegislationSourceReference("Monitorul Oficial 1/2026");

        Assert.NotEqual(sourceId.Value, rawDocumentId.Value);
        Assert.NotEqual(sourceReference.Value, rawDocumentId.Value);
        Assert.NotEqual(sourceDocumentId.Value, rawDocumentId.Value);
        Assert.NotEqual(rawDocumentId.Value, rawDocumentVersionId.Value);
    }

    [Fact]
    public void Raw_document_fingerprint_is_distinct_from_raw_document_identity()
    {
        var rawDocumentId = new RawDocumentId("RAW-DOC-1");
        var rawDocumentVersionId = new RawDocumentVersionId("RAW-DOC-1-V1");
        var fingerprint = new RawDocumentFingerprint(
            new RawDocumentContentHash("abc123"),
            new RawDocumentHashAlgorithm("sha256"));

        Assert.NotEqual(rawDocumentId.Value, fingerprint.ContentHash.Value);
        Assert.NotEqual(rawDocumentVersionId.Value, fingerprint.ContentHash.Value);
        Assert.Equal("sha256", fingerprint.Algorithm.Value);
    }

    [Fact]
    public void Raw_document_identity_terms_do_not_change_existing_legislation_document_id()
    {
        var existingId = new LegislationDocumentId("DOC-1");
        var rawDocumentId = new RawDocumentId("RAW-DOC-1");

        Assert.Equal("DOC-1", existingId.Value);
        Assert.Equal("RAW-DOC-1", rawDocumentId.Value);
        Assert.NotEqual(existingId.Value, rawDocumentId.Value);
    }

    [Fact]
    public void Legislation_source_metadata_stores_values()
    {
        var id = new LegislationSourceId("monitorul-oficial");

        var metadata = new LegislationSourceMetadata(
            id,
            " Monitorul Oficial ",
            " official-publication ");

        Assert.Same(id, metadata.Id);
        Assert.Equal("Monitorul Oficial", metadata.DisplayName);
        Assert.Equal("official-publication", metadata.SourceType);
    }

    [Fact]
    public void Legislation_source_metadata_rejects_empty_display_name()
    {
        Assert.Throws<ArgumentException>(() => new LegislationSourceMetadata(
            new LegislationSourceId("monitorul-oficial"),
            " ",
            "official-publication"));
    }

    [Fact]
    public void Legislation_source_metadata_rejects_empty_source_type()
    {
        Assert.Throws<ArgumentException>(() => new LegislationSourceMetadata(
            new LegislationSourceId("monitorul-oficial"),
            "Monitorul Oficial",
            " "));
    }

    [Fact]
    public void Raw_legislation_document_stores_its_values()
    {
        var document = new RawLegislationDocument(
            new LegislationDocumentId("DOC-1"),
            new LegislationSourceReference("Monitorul Oficial 1/2026"),
            "raw legislative text");

        Assert.Equal("DOC-1", document.Id.Value);
        Assert.Equal("Monitorul Oficial 1/2026", document.Source.Value);
        Assert.Equal("raw legislative text", document.Content);
    }

    [Fact]
    public void Ingestion_trace_entry_stores_its_values()
    {
        var timestamp = new DateTimeOffset(2026, 6, 4, 9, 0, 0, TimeSpan.Zero);

        var entry = new IngestionTraceEntry(
            IngestionStage.Normalization,
            IngestionStatus.Succeeded,
            timestamp,
            "normalized document text");

        Assert.Equal(IngestionStage.Normalization, entry.Stage);
        Assert.Equal(IngestionStatus.Succeeded, entry.Status);
        Assert.Equal(timestamp, entry.Timestamp);
        Assert.Equal("normalized document text", entry.Description);
    }

    [Fact]
    public void Ingestion_result_stores_values_and_reports_success()
    {
        var entry = new IngestionTraceEntry(
            IngestionStage.CuratedPromotion,
            IngestionStatus.Succeeded,
            new DateTimeOffset(2026, 6, 4, 10, 0, 0, TimeSpan.Zero),
            "promoted");

        var result = new IngestionResult(
            new IngestionBatchId("BATCH-1"),
            IngestionStatus.Succeeded,
            new[] { entry });

        Assert.Equal("BATCH-1", result.BatchId.Value);
        Assert.Equal(IngestionStatus.Succeeded, result.Status);
        Assert.Single(result.Trace);
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Ingestion_result_reports_non_success_for_other_statuses()
    {
        var result = new IngestionResult(
            new IngestionBatchId("BATCH-2"),
            IngestionStatus.Failed,
            Array.Empty<IngestionTraceEntry>());

        Assert.False(result.IsSuccessful);
    }
}
