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
