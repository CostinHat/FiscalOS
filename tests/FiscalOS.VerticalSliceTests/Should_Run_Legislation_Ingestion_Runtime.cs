using System;
using System.IO;
using System.Threading.Tasks;
using FiscalOS.Domain.LegislationIngestion;
using FiscalOS.Runtime.LegislationIngestion;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Legislation_Ingestion_Runtime
{
    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static RawLegislationDocument Document(string id) =>
        new(
            new LegislationDocumentId(id),
            new LegislationSourceReference("Monitorul Oficial 1/2026"),
            "raw legislative text");

    private static IngestionContext Context(string batchId = "BATCH-1") =>
        new(
            new IngestionBatchId(batchId),
            Array.Empty<RawLegislationDocument>(),
            Array.Empty<IngestionTraceEntry>());

    [Fact]
    public async Task Source_acquisition_pipeline_stores_raw_documents()
    {
        var document = Document("DOC-1");
        var source = new InMemoryLegislationSource(document);
        var repository = new InMemoryRawLegislationDocumentRepository();
        ILegislationIngestionPipeline pipeline = new LegislationIngestionPipeline(
            new ILegislationIngestionStage[]
            {
                new DiscoverLegislationDocumentsStage(() => At),
                new AcquireLegislationDocumentsStage(source, () => At),
                new ValidateRawLegislationDocumentsStage(() => At),
                new VersionLegislationDocumentsStage(() => At),
                new StoreRawLegislationDocumentsStage(repository, () => At),
            },
            () => At);

        var result = await pipeline.RunAsync(Context());

        var stored = await repository.GetAsync(new LegislationDocumentId("DOC-1"));
        Assert.True(result.IsSuccessful);
        Assert.Equal(IngestionStatus.Succeeded, result.Status);
        Assert.Equal("BATCH-1", result.BatchId.Value);
        Assert.Same(document, stored);
        Assert.Equal(5, result.Trace.Count);
        Assert.Equal(IngestionStage.Discovery, result.Trace[0].Stage);
        Assert.Equal(IngestionStage.Acquisition, result.Trace[1].Stage);
        Assert.Equal(IngestionStage.Normalization, result.Trace[2].Stage);
        Assert.Equal(IngestionStage.Versioning, result.Trace[3].Stage);
        Assert.Equal(IngestionStage.CuratedPromotion, result.Trace[4].Stage);
        Assert.Equal(4, result.Provenance.Count);
        Assert.Equal(5, result.AuditEvents.Count);
        Assert.Equal("BATCH-1:provenance:batch-started", result.Provenance[0].Id.Value);
        Assert.Equal("BATCH-1:provenance:source-selected:in-memory-legislation-source", result.Provenance[1].Id.Value);
        Assert.Equal("BATCH-1:provenance:candidate-fetched:DOC-1", result.Provenance[2].Id.Value);
        Assert.Equal("BATCH-1:provenance:batch-completed", result.Provenance[3].Id.Value);
        Assert.Equal(IngestionProvenanceCategory.Batch, result.Provenance[0].Category);
        Assert.Equal(IngestionProvenanceCategory.Source, result.Provenance[1].Category);
        Assert.Equal(IngestionProvenanceCategory.RawDocument, result.Provenance[2].Category);
        Assert.Equal(IngestionProvenanceCategory.Batch, result.Provenance[3].Category);
        Assert.Equal("in-memory-legislation-source", result.Provenance[1].SourceId!.Value);
        Assert.Null(result.Provenance[1].SourceMetadataSnapshotId);
        Assert.Null(result.Provenance[2].RawDocumentId);
        Assert.Equal("Candidate fetched: DOC-1.", result.Provenance[2].Description);
        Assert.Equal(IngestionAuditEventKind.BatchStarted, result.AuditEvents[0].Kind);
        Assert.Equal(IngestionAuditEventKind.SourceSelected, result.AuditEvents[1].Kind);
        Assert.Equal(IngestionAuditEventKind.SourceAcquisitionStarted, result.AuditEvents[2].Kind);
        Assert.Equal(IngestionAuditEventKind.CandidateFetched, result.AuditEvents[3].Kind);
        Assert.Equal(IngestionAuditEventKind.BatchCompleted, result.AuditEvents[4].Kind);
        Assert.Equal(IngestionAuditEventOutcome.Completed, result.AuditEvents[0].Outcome);
        Assert.Equal(IngestionAuditEventOutcome.Completed, result.AuditEvents[1].Outcome);
        Assert.Equal(IngestionAuditEventOutcome.Completed, result.AuditEvents[2].Outcome);
        Assert.Equal(IngestionAuditEventOutcome.Completed, result.AuditEvents[3].Outcome);
        Assert.Equal(IngestionAuditEventOutcome.Completed, result.AuditEvents[4].Outcome);
        Assert.Equal("in-memory-legislation-source", result.AuditEvents[1].SourceId!.Value);
        Assert.Equal("in-memory-legislation-source", result.AuditEvents[2].SourceId!.Value);
        Assert.Null(result.AuditEvents[1].SourceMetadataSnapshotId);
        Assert.Null(result.AuditEvents[2].SourceMetadataSnapshotId);
        Assert.Null(result.AuditEvents[3].RawDocumentId);
        Assert.Equal("Candidate fetched: DOC-1.", result.AuditEvents[3].Details);
        Assert.All(result.Provenance, record => Assert.Null(record.CorrelationId));
        Assert.All(result.AuditEvents, record => Assert.Null(record.CausationId));
    }

    [Fact]
    public async Task Successful_ingestion_path_handles_multiple_documents()
    {
        var first = Document("DOC-1");
        var second = Document("DOC-2");
        var source = new InMemoryLegislationSource(first, second);
        var repository = new InMemoryRawLegislationDocumentRepository();
        var pipeline = new LegislationIngestionPipeline(
            new ILegislationIngestionStage[]
            {
                new DiscoverLegislationDocumentsStage(() => At),
                new AcquireLegislationDocumentsStage(source, () => At),
                new ValidateRawLegislationDocumentsStage(() => At),
                new VersionLegislationDocumentsStage(() => At),
                new StoreRawLegislationDocumentsStage(repository, () => At),
            },
            () => At);

        var result = await pipeline.RunAsync(Context("BATCH-2"));

        var storedFirst = await repository.GetAsync(new LegislationDocumentId("DOC-1"));
        var storedSecond = await repository.GetAsync(new LegislationDocumentId("DOC-2"));
        Assert.True(result.IsSuccessful);
        Assert.Same(first, storedFirst);
        Assert.Same(second, storedSecond);
        Assert.Contains(result.Trace, entry => entry.Description == "Discovered legislation ingestion batch.");
        Assert.Contains(result.Trace, entry => entry.Description == "Acquired 2 document(s).");
        Assert.Contains(result.Trace, entry => entry.Description == "Validated 2 raw legislation document(s).");
        Assert.Contains(result.Trace, entry => entry.Description == "Versioned 2 raw legislation document(s).");
        Assert.Contains(result.Trace, entry => entry.Description == "Stored 2 raw legislation document(s).");
        Assert.Contains(result.Provenance, record =>
            record.Id.Value == "BATCH-2:provenance:candidate-fetched:DOC-1" &&
            record.RawDocumentId is null);
        Assert.Contains(result.Provenance, record =>
            record.Id.Value == "BATCH-2:provenance:candidate-fetched:DOC-2" &&
            record.RawDocumentId is null);
        Assert.Contains(result.AuditEvents, record =>
            record.Id.Value == "BATCH-2:audit:candidate-fetched:DOC-1" &&
            record.Kind == IngestionAuditEventKind.CandidateFetched &&
            record.RawDocumentId is null);
        Assert.Contains(result.AuditEvents, record =>
            record.Id.Value == "BATCH-2:audit:candidate-fetched:DOC-2" &&
            record.Kind == IngestionAuditEventKind.CandidateFetched &&
            record.RawDocumentId is null);
    }

    [Fact]
    public async Task Failure_path_returns_failed_result_and_preserves_prior_trace()
    {
        var source = new InMemoryLegislationSource(Document("DOC-1"));
        var repository = new InMemoryRawLegislationDocumentRepository();
        var pipeline = new LegislationIngestionPipeline(
            new ILegislationIngestionStage[]
            {
                new DiscoverLegislationDocumentsStage(() => At),
                new AcquireLegislationDocumentsStage(source, () => At),
                new ValidateRawLegislationDocumentsStage(() => At),
                new VersionLegislationDocumentsStage(() => At),
                new FailingLegislationIngestionStage(IngestionStage.CuratedPromotion, "storage failed"),
                new StoreRawLegislationDocumentsStage(repository, () => At),
            },
            () => At);

        var result = await pipeline.RunAsync(Context("BATCH-FAILED"));

        var stored = await repository.GetAsync(new LegislationDocumentId("DOC-1"));
        Assert.False(result.IsSuccessful);
        Assert.Equal(IngestionStatus.Failed, result.Status);
        Assert.Equal("BATCH-FAILED", result.BatchId.Value);
        Assert.Null(stored);
        Assert.Equal(5, result.Trace.Count);
        Assert.Equal(IngestionStage.Discovery, result.Trace[0].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[0].Status);
        Assert.Equal(IngestionStage.Acquisition, result.Trace[1].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[1].Status);
        Assert.Equal(IngestionStage.Normalization, result.Trace[2].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[2].Status);
        Assert.Equal(IngestionStage.Versioning, result.Trace[3].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[3].Status);
        Assert.Equal(IngestionStage.CuratedPromotion, result.Trace[4].Stage);
        Assert.Equal(IngestionStatus.Failed, result.Trace[4].Status);
        Assert.Equal("storage failed", result.Trace[4].Description);
        Assert.Equal(4, result.Provenance.Count);
        Assert.Equal(5, result.AuditEvents.Count);
        Assert.Equal("BATCH-FAILED:provenance:batch-started", result.Provenance[0].Id.Value);
        Assert.Equal("BATCH-FAILED:provenance:source-selected:in-memory-legislation-source", result.Provenance[1].Id.Value);
        Assert.Equal("BATCH-FAILED:provenance:candidate-fetched:DOC-1", result.Provenance[2].Id.Value);
        Assert.Equal("BATCH-FAILED:provenance:batch-failed", result.Provenance[3].Id.Value);
        Assert.Equal("storage failed", result.Provenance[3].Description);
        Assert.Equal("in-memory-legislation-source", result.Provenance[1].SourceId!.Value);
        Assert.Null(result.Provenance[2].RawDocumentId);
        Assert.Equal(IngestionAuditEventKind.BatchStarted, result.AuditEvents[0].Kind);
        Assert.Equal(IngestionAuditEventKind.SourceSelected, result.AuditEvents[1].Kind);
        Assert.Equal(IngestionAuditEventKind.SourceAcquisitionStarted, result.AuditEvents[2].Kind);
        Assert.Equal(IngestionAuditEventKind.CandidateFetched, result.AuditEvents[3].Kind);
        Assert.Equal(IngestionAuditEventKind.FailureRecorded, result.AuditEvents[4].Kind);
        Assert.Equal(IngestionAuditEventOutcome.Completed, result.AuditEvents[0].Outcome);
        Assert.Equal(IngestionAuditEventOutcome.Completed, result.AuditEvents[1].Outcome);
        Assert.Equal(IngestionAuditEventOutcome.Completed, result.AuditEvents[2].Outcome);
        Assert.Equal(IngestionAuditEventOutcome.Completed, result.AuditEvents[3].Outcome);
        Assert.Equal(IngestionAuditEventOutcome.Failed, result.AuditEvents[4].Outcome);
        Assert.Null(result.AuditEvents[3].RawDocumentId);
        Assert.Equal("storage failed", result.AuditEvents[4].Details);
        Assert.Equal("in-memory-legislation-source", result.AuditEvents[1].SourceId!.Value);
        Assert.Equal("in-memory-legislation-source", result.AuditEvents[2].SourceId!.Value);
    }

    [Fact]
    public void Default_emission_id_policy_preserves_existing_batch_id_format()
    {
        var policy = new DefaultIngestionEmissionIdPolicy();
        var batchId = new IngestionBatchId("BATCH-1");

        var provenanceId = policy.CreateProvenanceId(batchId, " batch-started ");
        var auditEventId = policy.CreateAuditEventId(batchId, " batch-started ");

        Assert.Equal("BATCH-1:provenance:batch-started", provenanceId.Value);
        Assert.Equal("BATCH-1:audit:batch-started", auditEventId.Value);
    }

    [Fact]
    public async Task Equivalent_batch_reruns_preserve_emitted_record_ids()
    {
        var firstPipeline = new LegislationIngestionPipeline(
            new ILegislationIngestionStage[]
            {
                new DiscoverLegislationDocumentsStage(() => At),
            },
            () => At);
        var secondPipeline = new LegislationIngestionPipeline(
            new ILegislationIngestionStage[]
            {
                new DiscoverLegislationDocumentsStage(() => At),
            },
            () => At);

        var first = await firstPipeline.RunAsync(Context("BATCH-RERUN"));
        var second = await secondPipeline.RunAsync(Context("BATCH-RERUN"));

        Assert.Equal(
            first.Provenance.Select(record => record.Id.Value),
            second.Provenance.Select(record => record.Id.Value));
        Assert.Equal(
            first.AuditEvents.Select(record => record.Id.Value),
            second.AuditEvents.Select(record => record.Id.Value));
    }

    [Fact]
    public async Task In_memory_repository_replaces_documents_by_id()
    {
        var repository = new InMemoryRawLegislationDocumentRepository();
        var first = Document("DOC-1");
        var second = new RawLegislationDocument(
            new LegislationDocumentId("DOC-1"),
            new LegislationSourceReference("Monitorul Oficial 2/2026"),
            "updated raw legislative text");

        await repository.StoreAsync(first);
        await repository.StoreAsync(second);

        var stored = await repository.GetAsync(new LegislationDocumentId("DOC-1"));
        var missing = await repository.GetAsync(new LegislationDocumentId("MISSING"));
        Assert.Same(second, stored);
        Assert.Null(missing);
    }

    [Fact]
    public void Runtime_project_preserves_dependency_direction_to_domain()
    {
        var root = RepositoryRoot();
        var domainProject = File.ReadAllText(Path.Combine(root, "src", "FiscalOS.Domain", "FiscalOS.Domain.csproj"));
        var runtimeProject = File.ReadAllText(Path.Combine(root, "src", "FiscalOS.Runtime", "FiscalOS.Runtime.csproj"));

        Assert.DoesNotContain("FiscalOS.Runtime", domainProject);
        Assert.Contains("FiscalOS.Domain", runtimeProject);
    }

    private static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "FiscalOS.sln")))
        {
            current = current.Parent;
        }

        if (current is null)
        {
            throw new InvalidOperationException("Could not locate repository root.");
        }

        return current.FullName;
    }
}
