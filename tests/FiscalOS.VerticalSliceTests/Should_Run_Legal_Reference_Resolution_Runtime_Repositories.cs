using System;
using System.Linq;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Legal_Reference_Resolution_Runtime_Repositories
{
    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ResolutionResult Unresolved(LegalReference query, string reason = "No stored resolution found.") =>
        new(ResolutionStatus.Unresolved, Array.Empty<ResolutionCandidate>(), new UnresolvedReference(query, reason));

    private static ResolutionAuditEntry AuditEntry(LegalReference query, ResolutionStatus status) =>
        new(query, new ResolutionDecision(status, status == ResolutionStatus.Resolved ? Selected : null), new ResolutionEvidence($"audit {status}"), At);

    private static ProvenanceStep Step(string source, string description) =>
        new(new ProvenanceSource(source), description);

    private static ResolutionProvenance Provenance(LegalReference query, ResolutionStatus status, string source) =>
        new(query, status, new ProvenanceChain(new[] { Step(source, $"provenance {status}") }), new ResolutionAuditTrail(Array.Empty<ResolutionAuditEntry>()));

    private static ResolutionEvidencePackage Package(ResolutionResult result, ResolutionProvenance provenance)
    {
        var trail = new ResolutionAuditTrail(new[] { AuditEntry(provenance.Query, result.Status) });
        return new ResolutionEvidencePackage(result, trail, provenance);
    }

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));
    private static readonly FullyQualifiedLegalReference Selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));

    [Fact]
    public async Task Resolution_repository_stores_retrieves_and_queries_outcomes()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        var resolved = Resolved(Selected);
        var unresolved = Unresolved(QueryArticle48);

        await repository.StoreAsync(QueryParagraph, resolved);
        await repository.StoreAsync(QueryArticle48, unresolved);

        var stored = await repository.GetAsync(QueryParagraph);
        var resolvedResults = await repository.GetByStatusAsync(ResolutionStatus.Resolved);
        var unresolvedReferences = await repository.GetUnresolvedAsync();
        var missing = await repository.GetAsync(Address(Seg("Article", "1")));

        Assert.Same(resolved, stored);
        Assert.Single(resolvedResults);
        Assert.Same(resolved, resolvedResults[0]);
        Assert.Single(unresolvedReferences);
        Assert.Equal(QueryArticle48, unresolvedReferences[0].Query);
        Assert.Null(missing);
    }

    [Fact]
    public async Task Resolution_repository_replaces_existing_result_for_same_query()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        var first = Unresolved(QueryParagraph);
        var second = Resolved(Selected);

        await repository.StoreAsync(QueryParagraph, first);
        await repository.StoreAsync(QueryParagraph, second);

        var stored = await repository.GetAsync(QueryParagraph);
        var unresolved = await repository.GetUnresolvedAsync();

        Assert.Same(second, stored);
        Assert.Empty(unresolved);
    }

    [Fact]
    public async Task Audit_repository_appends_trails_and_queries_entries()
    {
        var repository = new InMemoryLegalReferenceResolutionAuditRepository();
        var resolvedEntry = AuditEntry(QueryParagraph, ResolutionStatus.Resolved);
        var unresolvedEntry = AuditEntry(QueryArticle48, ResolutionStatus.Unresolved);

        await repository.StoreAsync(new ResolutionAuditTrail(new[] { resolvedEntry }));
        await repository.StoreAsync(new ResolutionAuditTrail(new[] { unresolvedEntry }));

        var byQuery = await repository.GetAsync(QueryParagraph);
        var resolved = await repository.GetByDecisionStatusAsync(ResolutionStatus.Resolved);
        var unresolved = await repository.GetUnresolvedAsync();
        var missing = await repository.GetAsync(Address(Seg("Article", "1")));

        Assert.NotNull(byQuery);
        Assert.Single(byQuery!.Entries);
        Assert.Same(resolvedEntry, byQuery.Entries[0]);
        Assert.Single(resolved);
        Assert.Same(resolvedEntry, resolved[0]);
        Assert.Single(unresolved);
        Assert.Same(unresolvedEntry, unresolved[0]);
        Assert.Null(missing);
    }

    [Fact]
    public async Task Provenance_repository_stores_retrieves_and_queries_sources_and_chains()
    {
        var repository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        var resolved = Provenance(QueryParagraph, ResolutionStatus.Resolved, "Monitorul Oficial");
        var unresolved = Provenance(QueryArticle48, ResolutionStatus.Unresolved, "resolution-runtime");

        await repository.StoreAsync(resolved);
        await repository.StoreAsync(unresolved);

        var stored = await repository.GetAsync(QueryParagraph);
        var sourceMatches = await repository.GetBySourceAsync("Monitorul Oficial");
        var chains = await repository.GetChainsAsync();
        var missing = await repository.GetAsync(Address(Seg("Article", "1")));

        Assert.Same(resolved, stored);
        Assert.Single(sourceMatches);
        Assert.Same(resolved, sourceMatches[0]);
        Assert.Equal(2, chains.Count);
        Assert.Contains(chains, chain => chain.Steps.Any(step => step.Source.Value == "resolution-runtime"));
        Assert.Null(missing);
    }

    [Fact]
    public async Task Provenance_repository_replaces_existing_provenance_for_same_query()
    {
        var repository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        var first = Provenance(QueryParagraph, ResolutionStatus.Unresolved, "first");
        var second = Provenance(QueryParagraph, ResolutionStatus.Resolved, "second");

        await repository.StoreAsync(first);
        await repository.StoreAsync(second);

        var stored = await repository.GetAsync(QueryParagraph);
        var chains = await repository.GetChainsAsync();

        Assert.Same(second, stored);
        Assert.Single(chains);
        Assert.Equal("second", chains[0].Steps[0].Source.Value);
    }

    [Fact]
    public async Task Evidence_package_repository_stores_retrieves_and_replaces_packages()
    {
        var repository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();
        var first = Package(Unresolved(QueryParagraph), Provenance(QueryParagraph, ResolutionStatus.Unresolved, "first"));
        var second = Package(Resolved(Selected), Provenance(QueryParagraph, ResolutionStatus.Resolved, "second"));

        await repository.StoreAsync(QueryParagraph, first);
        await repository.StoreAsync(QueryParagraph, second);

        var stored = await repository.GetAsync(QueryParagraph);
        var missing = await repository.GetAsync(QueryArticle48);

        Assert.Same(second, stored);
        Assert.Equal(ResolutionStatus.Resolved, stored!.Result.Status);
        Assert.Null(missing);
    }

    [Fact]
    public async Task Runtime_repositories_wire_into_resolution_audit_provenance_and_package_verticals()
    {
        var resolutionRepository = new InMemoryLegalReferenceResolutionRepository();
        await resolutionRepository.StoreAsync(QueryParagraph, Resolved(Selected));

        ILegalReferenceResolutionEngine resolutionEngine = new LegalReferenceResolutionEngine(
            new LegalReferenceResolutionPipeline(
                new RepositoryLegalReferenceResolutionStage(resolutionRepository)));

        var auditRepository = new InMemoryLegalReferenceResolutionAuditRepository();
        ILegalReferenceResolutionAuditEngine auditEngine = new LegalReferenceResolutionAuditEngine(
            new LegalReferenceResolutionAuditPipeline(
                new PersistResolutionAuditTrailStage(auditRepository)),
            () => At);

        var provenanceRepository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        ILegalReferenceResolutionProvenanceEngine provenanceEngine = new LegalReferenceResolutionProvenanceEngine(
            new LegalReferenceResolutionProvenancePipeline(
                new PersistResolutionProvenanceStage(provenanceRepository)));

        var packageRepository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();
        ILegalReferenceResolutionEvidencePackageEngine packageEngine = new LegalReferenceResolutionEvidencePackageEngine(
            new LegalReferenceResolutionEvidencePackagePipeline(
                new PersistResolutionEvidencePackageStage(packageRepository)));

        var results = await resolutionEngine.ResolveAsync(new[] { QueryParagraph, QueryArticle48 });
        var auditTrail = await auditEngine.AuditAsync(results);
        var provenances = results.Select(result =>
        {
            var query = result.ResolvedReference?.Reference ?? result.Unresolved!.Query;
            return Provenance(query, result.Status, "resolution-runtime");
        }).ToList();
        await provenanceEngine.BuildAsync(provenances);
        var packages = new ResolutionEvidencePackageComposer().Compose(results, auditTrail, provenances);

        var processed = await packageEngine.ProcessAsync(packages);

        var storedAudit = await auditRepository.GetByDecisionStatusAsync(ResolutionStatus.Resolved);
        var storedProvenance = await provenanceRepository.GetAsync(QueryParagraph);
        var storedPackage = await packageRepository.GetAsync(QueryParagraph);

        Assert.Equal(2, results.Count);
        Assert.Equal(2, processed.Count);
        Assert.Single(storedAudit);
        Assert.NotNull(storedProvenance);
        Assert.NotNull(storedPackage);
        Assert.Same(packages[0], storedPackage);
    }
}
