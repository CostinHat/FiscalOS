using System;
using System.Threading.Tasks;
using FiscalOS.Domain.LegislationIngestion;
using FiscalOS.Runtime.LegislationIngestion;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Discover_Legislation_Ingestion_Runtime
{
    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static RawLegislationDocument Document(string id) =>
        new(
            new LegislationDocumentId(id),
            new LegislationSourceReference("Monitorul Oficial 1/2026"),
            "raw legislative text");

    private static IngestionContext Context(params RawLegislationDocument[] documents) =>
        new(
            new IngestionBatchId("BATCH-DISCOVERY"),
            documents,
            Array.Empty<IngestionTraceEntry>());

    [Fact]
    public async Task Discovery_adds_exactly_one_success_trace()
    {
        var stage = new DiscoverLegislationDocumentsStage(() => At);

        var result = await stage.ExecuteAsync(Context());

        Assert.Single(result.Trace);
        Assert.Equal(IngestionStage.Discovery, result.Trace[0].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[0].Status);
        Assert.Equal("Discovered legislation ingestion batch.", result.Trace[0].Description);
    }

    [Fact]
    public async Task Discovery_leaves_documents_unchanged()
    {
        var document = Document("DOC-1");
        var stage = new DiscoverLegislationDocumentsStage(() => At);

        var result = await stage.ExecuteAsync(Context(document));

        Assert.Single(result.Documents);
        Assert.Same(document, result.Documents[0]);
        Assert.Equal(document.Id, result.Documents[0].Id);
    }

    [Fact]
    public async Task Composed_runtime_emits_discovery_before_acquisition_validation_and_storage()
    {
        var source = new InMemoryLegislationSource(Document("DOC-1"));
        var repository = new InMemoryRawLegislationDocumentRepository();
        var runtime = LegislationIngestionRuntime.Create(source, repository, () => At);

        var result = await runtime.RunAsync(new IngestionBatchId("BATCH-1"));

        Assert.True(result.IsSuccessful);
        Assert.Equal(4, result.Trace.Count);
        Assert.Equal(IngestionStage.Discovery, result.Trace[0].Stage);
        Assert.Equal(IngestionStage.Acquisition, result.Trace[1].Stage);
        Assert.Equal(IngestionStage.Normalization, result.Trace[2].Stage);
        Assert.Equal(IngestionStage.CuratedPromotion, result.Trace[3].Stage);
    }

    [Fact]
    public async Task Failing_stage_after_discovery_preserves_discovery_trace()
    {
        var pipeline = new LegislationIngestionPipeline(
            new ILegislationIngestionStage[]
            {
                new DiscoverLegislationDocumentsStage(() => At),
                new FailingLegislationIngestionStage(IngestionStage.Acquisition, "acquisition failed"),
            },
            () => At);

        var result = await pipeline.RunAsync(Context());

        Assert.False(result.IsSuccessful);
        Assert.Equal(IngestionStatus.Failed, result.Status);
        Assert.Equal(2, result.Trace.Count);
        Assert.Equal(IngestionStage.Discovery, result.Trace[0].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[0].Status);
        Assert.Equal(IngestionStage.Acquisition, result.Trace[1].Stage);
        Assert.Equal(IngestionStatus.Failed, result.Trace[1].Status);
        Assert.Equal("acquisition failed", result.Trace[1].Description);
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
