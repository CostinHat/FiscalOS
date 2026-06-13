using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using FiscalOS.Runtime.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Process_Evidence_Packages_Through_Runtime_Evidence_Package_Vertical
{
    private sealed class InMemoryLegalReferenceResolutionRepository : ILegalReferenceResolutionRepository
    {
        private readonly Dictionary<LegalReference, ResolutionResult> _store = new();

        public Task StoreAsync(LegalReference query, ResolutionResult result, CancellationToken cancellationToken = default)
        {
            _store[query] = result;
            return Task.CompletedTask;
        }

        public Task<ResolutionResult?> GetAsync(LegalReference query, CancellationToken cancellationToken = default)
            => Task.FromResult<ResolutionResult?>(_store.TryGetValue(query, out var result) ? result : null);

        public Task<IReadOnlyList<ResolutionResult>> GetByStatusAsync(ResolutionStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ResolutionResult>>(_store.Values.Where(result => result.Status == status).ToList());

        public Task<IReadOnlyList<UnresolvedReference>> GetUnresolvedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UnresolvedReference>>(
                _store.Values.Where(result => result.Unresolved is not null).Select(result => result.Unresolved!).ToList());
    }

    private sealed class InMemoryLegalReferenceResolutionProvenanceRepository : ILegalReferenceResolutionProvenanceRepository
    {
        private readonly Dictionary<LegalReference, ResolutionProvenance> _store = new();

        public Task StoreAsync(ResolutionProvenance provenance, CancellationToken cancellationToken = default)
        {
            _store[provenance.Query] = provenance;
            return Task.CompletedTask;
        }

        public Task<ResolutionProvenance?> GetAsync(LegalReference query, CancellationToken cancellationToken = default)
            => Task.FromResult<ResolutionProvenance?>(_store.TryGetValue(query, out var provenance) ? provenance : null);

        public Task<IReadOnlyList<ResolutionProvenance>> GetBySourceAsync(string source, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ResolutionProvenance>>(
                _store.Values.Where(provenance => provenance.Chain.Steps.Any(step => step.Source.Value == source)).ToList());

        public Task<IReadOnlyList<ProvenanceChain>> GetChainsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProvenanceChain>>(_store.Values.Select(provenance => provenance.Chain).ToList());
    }

    private sealed class InMemoryLegalReferenceResolutionEvidencePackageRepository : ILegalReferenceResolutionEvidencePackageRepository
    {
        private readonly Dictionary<LegalReference, ResolutionEvidencePackage> _store = new();

        public Task StoreAsync(LegalReference query, ResolutionEvidencePackage package, CancellationToken cancellationToken = default)
        {
            _store[query] = package;
            return Task.CompletedTask;
        }

        public Task<ResolutionEvidencePackage?> GetAsync(LegalReference query, CancellationToken cancellationToken = default)
            => Task.FromResult<ResolutionEvidencePackage?>(_store.TryGetValue(query, out var package) ? package : null);
    }

    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ProvenanceStep Step(string source, string description) =>
        new(new ProvenanceSource(source), description);

    private static ResolutionProvenance ProvenanceFor(ResolutionResult result, ResolutionAuditTrail auditTrail)
    {
        var query = result.ResolvedReference?.Reference ?? result.Unresolved!.Query;
        var step = result.IsResolved
            ? Step("resolution-runtime", "resolved from repository result")
            : Step("resolution-runtime", "recorded unresolved repository miss");

        return new ResolutionProvenance(
            query,
            result.Status,
            new ProvenanceChain(new[] { step }),
            auditTrail);
    }

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    [Fact]
    public async Task Runtime_evidence_package_engine_processes_composed_resolution_audit_and_provenance_outputs()
    {
        var resolutionRepository = new InMemoryLegalReferenceResolutionRepository();
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        await resolutionRepository.StoreAsync(QueryParagraph, Resolved(selected));

        ILegalReferenceResolutionEngine resolutionEngine = new LegalReferenceResolutionEngine(
            new LegalReferenceResolutionPipeline(
                new RepositoryLegalReferenceResolutionStage(resolutionRepository)));
        ILegalReferenceResolutionAuditEngine auditEngine = new LegalReferenceResolutionAuditEngine(
            new LegalReferenceResolutionAuditPipeline(),
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
        var provenances = results.Select(result => ProvenanceFor(result, auditTrail)).ToList();
        var chain = await provenanceEngine.BuildAsync(provenances);
        var packages = new ResolutionEvidencePackageComposer().Compose(results, auditTrail, provenances);

        var processed = await packageEngine.ProcessAsync(packages);

        Assert.Equal(2, chain.Steps.Count);
        Assert.Equal(2, processed.Count);
        Assert.Same(packages[0], processed[0]);
        Assert.Same(packages[1], processed[1]);

        var storedResolved = await packageRepository.GetAsync(QueryParagraph);
        var storedUnresolved = await packageRepository.GetAsync(QueryArticle48);

        Assert.NotNull(storedResolved);
        Assert.NotNull(storedUnresolved);
        Assert.Equal(ResolutionStatus.Resolved, storedResolved!.Result.Status);
        Assert.Equal(ResolutionStatus.Unresolved, storedUnresolved!.Result.Status);
        Assert.Same(provenances[0], storedResolved.Provenance);
        Assert.Same(provenances[1], storedUnresolved.Provenance);
        Assert.Single(storedResolved.AuditTrail.Entries);
        Assert.Single(storedUnresolved.AuditTrail.Entries);
    }
}
