using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Register_Legal_Reference_Resolution_Runtime
{
    private static readonly DateTimeOffset At = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));
    private static readonly FullyQualifiedLegalReference Selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));

    [Fact]
    public void Service_collection_extension_resolves_runtime_and_graph_components()
    {
        var services = new ServiceCollection();
        services.AddLegalReferenceResolutionRuntime();

        using var provider = services.BuildServiceProvider();

        Assert.IsType<LegalReferenceResolutionRuntime>(provider.GetRequiredService<LegalReferenceResolutionRuntime>());
        Assert.IsType<InMemoryLegalReferenceResolutionRepository>(provider.GetRequiredService<ILegalReferenceResolutionRepository>());
        Assert.IsType<LegalReferenceResolutionPipeline>(provider.GetRequiredService<ILegalReferenceResolutionPipeline>());
        Assert.IsType<LegalReferenceResolutionEngine>(provider.GetRequiredService<ILegalReferenceResolutionEngine>());
        Assert.IsType<LegalReferenceResolutionAuditPipeline>(provider.GetRequiredService<ILegalReferenceResolutionAuditPipeline>());
        Assert.IsType<LegalReferenceResolutionAuditEngine>(provider.GetRequiredService<ILegalReferenceResolutionAuditEngine>());
        Assert.IsType<LegalReferenceResolutionProvenancePipeline>(provider.GetRequiredService<ILegalReferenceResolutionProvenancePipeline>());
        Assert.IsType<LegalReferenceResolutionProvenanceEngine>(provider.GetRequiredService<ILegalReferenceResolutionProvenanceEngine>());
        Assert.IsType<LegalReferenceResolutionEvidencePackagePipeline>(provider.GetRequiredService<ILegalReferenceResolutionEvidencePackagePipeline>());
        Assert.IsType<LegalReferenceResolutionEvidencePackageEngine>(provider.GetRequiredService<ILegalReferenceResolutionEvidencePackageEngine>());
        Assert.IsType<ResolutionEvidencePackageComposer>(provider.GetRequiredService<ResolutionEvidencePackageComposer>());
    }

    [Fact]
    public async Task Di_created_runtime_processes_known_reference_end_to_end()
    {
        var services = new ServiceCollection();
        services.AddLegalReferenceResolutionRuntime();

        using var provider = services.BuildServiceProvider();

        var resolutionRepository = provider.GetRequiredService<ILegalReferenceResolutionRepository>();
        await resolutionRepository.StoreAsync(QueryParagraph, Resolved(Selected));

        var runtime = provider.GetRequiredService<LegalReferenceResolutionRuntime>();
        var packages = await runtime.ResolveAsync(new[] { QueryParagraph });

        var auditRepository = provider.GetRequiredService<ILegalReferenceResolutionAuditRepository>();
        var provenanceRepository = provider.GetRequiredService<ILegalReferenceResolutionProvenanceRepository>();
        var packageRepository = provider.GetRequiredService<ILegalReferenceResolutionEvidencePackageRepository>();

        var auditTrail = await auditRepository.GetAsync(QueryParagraph);
        var provenance = await provenanceRepository.GetAsync(QueryParagraph);
        var storedPackage = await packageRepository.GetAsync(QueryParagraph);

        Assert.Single(packages);
        Assert.True(packages[0].Result.IsResolved);
        Assert.Equal(Selected, packages[0].Result.ResolvedReference);
        Assert.NotNull(auditTrail);
        Assert.Single(auditTrail!.Entries);
        Assert.Equal(ResolutionStatus.Resolved, auditTrail.Entries[0].Decision.Status);
        Assert.NotNull(provenance);
        Assert.Equal(QueryParagraph, provenance!.Query);
        Assert.NotNull(storedPackage);
        Assert.Same(packages[0], storedPackage);
    }

    [Fact]
    public void Runtime_graph_is_wired_consistently()
    {
        var services = new ServiceCollection();
        services.AddLegalReferenceResolutionRuntime();

        using var provider = services.BuildServiceProvider();

        Assert.Same(
            provider.GetRequiredService<ILegalReferenceResolutionRepository>(),
            provider.GetRequiredService<ILegalReferenceResolutionRepository>());
        Assert.Same(
            provider.GetRequiredService<ILegalReferenceResolutionPipeline>(),
            provider.GetRequiredService<ILegalReferenceResolutionPipeline>());
        Assert.Same(
            provider.GetRequiredService<ILegalReferenceResolutionEngine>(),
            provider.GetRequiredService<ILegalReferenceResolutionEngine>());
        Assert.Same(
            provider.GetRequiredService<ResolutionEvidencePackageComposer>(),
            provider.GetRequiredService<ResolutionEvidencePackageComposer>());
    }

    [Fact]
    public async Task Repository_override_is_honored_before_default_registration()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILegalReferenceResolutionRepository>(new CustomResolutionRepository(QueryArticle48, Selected));
        services.AddLegalReferenceResolutionRuntime();

        using var provider = services.BuildServiceProvider();

        var runtime = provider.GetRequiredService<LegalReferenceResolutionRuntime>();
        var packages = await runtime.ResolveAsync(new[] { QueryArticle48 });

        Assert.Single(packages);
        Assert.True(packages[0].Result.IsResolved);
        Assert.Equal(Selected, packages[0].Result.ResolvedReference);
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

    private sealed class CustomResolutionRepository : ILegalReferenceResolutionRepository
    {
        private readonly LegalReference _query;
        private readonly ResolutionResult _result;

        public CustomResolutionRepository(LegalReference query, FullyQualifiedLegalReference selected)
        {
            _query = query;
            _result = Resolved(selected);
        }

        public Task StoreAsync(LegalReference query, ResolutionResult result, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<ResolutionResult?> GetAsync(LegalReference query, CancellationToken cancellationToken = default)
            => Task.FromResult<ResolutionResult?>(_query.Equals(query) ? _result : null);

        public Task<IReadOnlyList<ResolutionResult>> GetByStatusAsync(ResolutionStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ResolutionResult>>(status == _result.Status ? new[] { _result } : Array.Empty<ResolutionResult>());

        public Task<IReadOnlyList<UnresolvedReference>> GetUnresolvedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UnresolvedReference>>(Array.Empty<UnresolvedReference>());
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
