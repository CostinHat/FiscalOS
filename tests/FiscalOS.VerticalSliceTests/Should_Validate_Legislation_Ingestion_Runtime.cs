using System;
using System.Threading.Tasks;
using FiscalOS.Domain.LegislationIngestion;
using FiscalOS.Runtime.LegislationIngestion;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Validate_Legislation_Ingestion_Runtime
{
    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static RawLegislationDocument Document(string id, string content) =>
        new(
            new LegislationDocumentId(id),
            new LegislationSourceReference("Monitorul Oficial 1/2026"),
            content);

    private static IngestionContext Context(params RawLegislationDocument[] documents) =>
        new(
            new IngestionBatchId("BATCH-VALIDATION"),
            documents,
            Array.Empty<IngestionTraceEntry>());

    [Fact]
    public async Task Valid_documents_add_validation_success_trace()
    {
        var stage = new ValidateRawLegislationDocumentsStage(() => At);

        var result = await stage.ExecuteAsync(Context(Document("DOC-1", "raw legislative text")));

        Assert.Single(result.Trace);
        Assert.Equal(IngestionStage.Normalization, result.Trace[0].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[0].Status);
        Assert.Equal("Validated 1 raw legislation document(s).", result.Trace[0].Description);
    }

    [Fact]
    public async Task Null_content_returns_failed_pipeline_result()
    {
        var result = await RunValidationPipeline(Document("DOC-NULL", null!));

        AssertValidationFailure(
            result,
            "Raw legislation document 'DOC-NULL' content is required.");
    }

    [Fact]
    public async Task Empty_content_returns_failed_pipeline_result()
    {
        var result = await RunValidationPipeline(Document("DOC-EMPTY", string.Empty));

        AssertValidationFailure(
            result,
            "Raw legislation document 'DOC-EMPTY' content cannot be empty.");
    }

    [Fact]
    public async Task Whitespace_content_returns_failed_pipeline_result()
    {
        var result = await RunValidationPipeline(Document("DOC-WHITESPACE", "   "));

        AssertValidationFailure(
            result,
            "Raw legislation document 'DOC-WHITESPACE' content cannot be whitespace.");
    }

    [Fact]
    public async Task Composed_ingestion_validates_before_storage()
    {
        var source = new InMemoryLegislationSource(Document("DOC-INVALID", "   "));
        var repository = new InMemoryRawLegislationDocumentRepository();
        var runtime = LegislationIngestionRuntime.Create(source, repository, () => At);

        var result = await runtime.RunAsync(new IngestionBatchId("BATCH-COMPOSED-VALIDATION"));

        var stored = await repository.GetAsync(new LegislationDocumentId("DOC-INVALID"));
        Assert.False(result.IsSuccessful);
        Assert.Equal(IngestionStatus.Failed, result.Status);
        Assert.Null(stored);
        Assert.Equal(3, result.Trace.Count);
        Assert.Equal(IngestionStage.Discovery, result.Trace[0].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[0].Status);
        Assert.Equal(IngestionStage.Acquisition, result.Trace[1].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[1].Status);
        Assert.Equal(IngestionStage.Normalization, result.Trace[2].Stage);
        Assert.Equal(IngestionStatus.Failed, result.Trace[2].Status);
        Assert.Equal(
            "Raw legislation document 'DOC-INVALID' content cannot be whitespace.",
            result.Trace[2].Description);
    }

    private static Task<IngestionResult> RunValidationPipeline(RawLegislationDocument document)
    {
        var pipeline = new LegislationIngestionPipeline(
            new ILegislationIngestionStage[]
            {
                new ValidateRawLegislationDocumentsStage(() => At),
            },
            () => At);

        return pipeline.RunAsync(Context(document));
    }

    private static void AssertValidationFailure(
        IngestionResult result,
        string expectedDescription)
    {
        Assert.False(result.IsSuccessful);
        Assert.Equal(IngestionStatus.Failed, result.Status);
        Assert.Single(result.Trace);
        Assert.Equal(IngestionStage.Normalization, result.Trace[0].Stage);
        Assert.Equal(IngestionStatus.Failed, result.Trace[0].Status);
        Assert.Equal(expectedDescription, result.Trace[0].Description);
    }
}
