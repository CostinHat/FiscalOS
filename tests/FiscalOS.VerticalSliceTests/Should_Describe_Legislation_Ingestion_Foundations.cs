using System;
using FiscalOS.Domain.LegislationIngestion;
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
