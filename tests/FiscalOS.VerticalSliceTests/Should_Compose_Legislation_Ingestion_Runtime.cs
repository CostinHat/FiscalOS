using System;
using System.IO;
using System.Threading.Tasks;
using FiscalOS.Domain.LegislationIngestion;
using FiscalOS.Runtime.LegislationIngestion;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Compose_Legislation_Ingestion_Runtime
{
    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static RawLegislationDocument Document(string id) =>
        new(
            new LegislationDocumentId(id),
            new LegislationSourceReference("Monitorul Oficial 1/2026"),
            "raw legislative text");

    [Fact]
    public async Task Successful_composed_ingestion_returns_succeeded_result()
    {
        var source = new InMemoryLegislationSource(Document("DOC-1"));
        var repository = new InMemoryRawLegislationDocumentRepository();
        var runtime = LegislationIngestionRuntime.Create(source, repository, () => At);

        var result = await runtime.RunAsync(new IngestionBatchId("BATCH-1"));

        Assert.True(result.IsSuccessful);
        Assert.Equal(IngestionStatus.Succeeded, result.Status);
        Assert.Equal("BATCH-1", result.BatchId.Value);
        Assert.Equal(3, result.Trace.Count);
        Assert.Equal(IngestionStage.Acquisition, result.Trace[0].Stage);
        Assert.Equal(IngestionStage.Normalization, result.Trace[1].Stage);
        Assert.Equal(IngestionStage.CuratedPromotion, result.Trace[2].Stage);
    }

    [Fact]
    public async Task Raw_documents_are_persisted_through_composed_runtime()
    {
        var first = Document("DOC-1");
        var second = Document("DOC-2");
        var source = new InMemoryLegislationSource(first, second);
        var repository = new InMemoryRawLegislationDocumentRepository();
        var runtime = LegislationIngestionRuntime.Create(source, repository, () => At);

        await runtime.RunAsync(new IngestionBatchId("BATCH-2"));

        var storedFirst = await repository.GetAsync(new LegislationDocumentId("DOC-1"));
        var storedSecond = await repository.GetAsync(new LegislationDocumentId("DOC-2"));
        Assert.Same(first, storedFirst);
        Assert.Same(second, storedSecond);
    }

    [Fact]
    public async Task Failed_stage_returns_failed_result()
    {
        var pipeline = new LegislationIngestionPipeline(
            new ILegislationIngestionStage[]
            {
                new FailingLegislationIngestionStage(IngestionStage.Discovery, "discovery failed"),
            },
            () => At);
        var runtime = new LegislationIngestionRuntime(pipeline);

        var result = await runtime.RunAsync(new IngestionBatchId("BATCH-FAILED"));

        Assert.False(result.IsSuccessful);
        Assert.Equal(IngestionStatus.Failed, result.Status);
        Assert.Single(result.Trace);
        Assert.Equal(IngestionStage.Discovery, result.Trace[0].Stage);
        Assert.Equal(IngestionStatus.Failed, result.Trace[0].Status);
        Assert.Equal("discovery failed", result.Trace[0].Description);
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
