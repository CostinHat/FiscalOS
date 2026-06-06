using System;
using System.IO;
using System.Threading.Tasks;
using FiscalOS.Domain.LegislationIngestion;
using FiscalOS.Runtime.LegislationIngestion;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Version_Legislation_Ingestion_Runtime
{
    private static readonly DateTimeOffset At = new(2026, 6, 6, 12, 0, 0, TimeSpan.Zero);

    private static RawLegislationDocument Document(string id) =>
        new(
            new LegislationDocumentId(id),
            new LegislationSourceReference("Monitorul Oficial 1/2026"),
            "raw legislative text");

    private static IngestionContext Context(params RawLegislationDocument[] documents) =>
        new(
            new IngestionBatchId("BATCH-VERSIONING"),
            documents,
            Array.Empty<IngestionTraceEntry>());

    [Fact]
    public async Task Versioning_adds_deterministic_success_trace()
    {
        var stage = new VersionLegislationDocumentsStage(() => At);

        var result = await stage.ExecuteAsync(Context(Document("DOC-1")));

        Assert.Single(result.Trace);
        Assert.Equal(IngestionStage.Versioning, result.Trace[0].Stage);
        Assert.Equal(IngestionStatus.Succeeded, result.Trace[0].Status);
        Assert.Equal("Versioned 1 raw legislation document(s).", result.Trace[0].Description);
    }

    [Fact]
    public async Task Versioning_does_not_mutate_documents()
    {
        var first = Document("DOC-1");
        var second = Document("DOC-2");
        var stage = new VersionLegislationDocumentsStage(() => At);

        var result = await stage.ExecuteAsync(Context(first, second));

        Assert.Equal(2, result.Documents.Count);
        Assert.Same(first, result.Documents[0]);
        Assert.Same(second, result.Documents[1]);
        Assert.Equal("DOC-1", result.Documents[0].Id.Value);
        Assert.Equal("DOC-2", result.Documents[1].Id.Value);
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
