using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Compose_Legal_Reference_Resolution_Runtime
{
    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

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
    public async Task Known_references_resolve_and_produce_complete_packages()
    {
        var resolutionRepository = new InMemoryLegalReferenceResolutionRepository();
        await resolutionRepository.StoreAsync(QueryParagraph, Resolved(Selected));
        var auditRepository = new InMemoryLegalReferenceResolutionAuditRepository();
        var provenanceRepository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        var packageRepository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();
        var runtime = RuntimeWith(resolutionRepository, auditRepository, provenanceRepository, packageRepository);

        var packages = await runtime.ResolveAsync(new[] { QueryParagraph });

        Assert.Single(packages);
        Assert.True(packages[0].Result.IsResolved);
        Assert.Equal(Selected, packages[0].Result.ResolvedReference);
        Assert.Single(packages[0].AuditTrail.Entries);
        Assert.Equal(ResolutionStatus.Resolved, packages[0].AuditTrail.Entries[0].Decision.Status);
        Assert.Equal(QueryParagraph, packages[0].Provenance.Query);
        Assert.Equal(ResolutionStatus.Resolved, packages[0].Provenance.Status);
        Assert.Single(packages[0].Provenance.Chain.Steps);
        Assert.Equal("resolution-runtime", packages[0].Provenance.Chain.Steps[0].Source.Value);
    }

    [Fact]
    public async Task Unknown_references_produce_unresolved_packages()
    {
        var runtime = RuntimeWith(
            new InMemoryLegalReferenceResolutionRepository(),
            new InMemoryLegalReferenceResolutionAuditRepository(),
            new InMemoryLegalReferenceResolutionProvenanceRepository(),
            new InMemoryLegalReferenceResolutionEvidencePackageRepository());

        var packages = await runtime.ResolveAsync(new[] { QueryArticle48 });

        Assert.Single(packages);
        Assert.True(packages[0].Result.IsUnresolved);
        Assert.Equal(QueryArticle48, packages[0].Result.Unresolved!.Query);
        Assert.Equal(QueryArticle48, packages[0].AuditTrail.Entries[0].Query);
        Assert.Equal(QueryArticle48, packages[0].Provenance.Query);
        Assert.Equal(ResolutionStatus.Unresolved, packages[0].Provenance.Status);
    }

    [Fact]
    public async Task Runtime_persists_audit_provenance_and_packages_through_configured_repositories()
    {
        var resolutionRepository = new InMemoryLegalReferenceResolutionRepository();
        await resolutionRepository.StoreAsync(QueryParagraph, Resolved(Selected));
        var auditRepository = new InMemoryLegalReferenceResolutionAuditRepository();
        var provenanceRepository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        var packageRepository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();
        var runtime = RuntimeWith(resolutionRepository, auditRepository, provenanceRepository, packageRepository);

        var packages = await runtime.ResolveAsync(new[] { QueryParagraph, QueryArticle48 });

        var resolvedAudit = await auditRepository.GetByDecisionStatusAsync(ResolutionStatus.Resolved);
        var unresolvedAudit = await auditRepository.GetUnresolvedAsync();
        var resolvedProvenance = await provenanceRepository.GetAsync(QueryParagraph);
        var unresolvedProvenance = await provenanceRepository.GetAsync(QueryArticle48);
        var resolvedPackage = await packageRepository.GetAsync(QueryParagraph);
        var unresolvedPackage = await packageRepository.GetAsync(QueryArticle48);

        Assert.Equal(2, packages.Count);
        Assert.Single(resolvedAudit);
        Assert.Single(unresolvedAudit);
        Assert.NotNull(resolvedProvenance);
        Assert.NotNull(unresolvedProvenance);
        Assert.NotNull(resolvedPackage);
        Assert.NotNull(unresolvedPackage);
        Assert.Same(packages[0], resolvedPackage);
        Assert.Same(packages[1], unresolvedPackage);
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

    private static LegalReferenceResolutionRuntime RuntimeWith(
        ILegalReferenceResolutionRepository resolutionRepository,
        ILegalReferenceResolutionAuditRepository auditRepository,
        ILegalReferenceResolutionProvenanceRepository provenanceRepository,
        ILegalReferenceResolutionEvidencePackageRepository packageRepository)
    {
        ILegalReferenceResolutionEngine resolutionEngine = new LegalReferenceResolutionEngine(
            new LegalReferenceResolutionPipeline(
                new RepositoryLegalReferenceResolutionStage(resolutionRepository)));
        ILegalReferenceResolutionAuditEngine auditEngine = new LegalReferenceResolutionAuditEngine(
            new LegalReferenceResolutionAuditPipeline(
                new PersistResolutionAuditTrailStage(auditRepository)),
            () => At);
        ILegalReferenceResolutionProvenanceEngine provenanceEngine = new LegalReferenceResolutionProvenanceEngine(
            new LegalReferenceResolutionProvenancePipeline(
                new PersistResolutionProvenanceStage(provenanceRepository)));
        ILegalReferenceResolutionEvidencePackageEngine packageEngine = new LegalReferenceResolutionEvidencePackageEngine(
            new LegalReferenceResolutionEvidencePackagePipeline(
                new PersistResolutionEvidencePackageStage(packageRepository)));

        return new LegalReferenceResolutionRuntime(
            resolutionEngine,
            auditEngine,
            provenanceEngine,
            packageEngine,
            new ResolutionEvidencePackageComposer());
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
