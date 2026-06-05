using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Evidence_Package_Pipeline_Contracts
{
    // Test-only doubles validating the contract shapes and semantics. Not
    // production implementations (no engine, repository implementation, or
    // resolution logic), per FOS-0054 scope.

    private sealed class AppendPackageStage : ILegalReferenceResolutionEvidencePackageStage
    {
        private readonly ResolutionEvidencePackage _package;

        public AppendPackageStage(ResolutionEvidencePackage package) => _package = package;

        public string Name => "append-package";

        public Task<LegalReferenceResolutionEvidencePackageContext> ExecuteAsync(LegalReferenceResolutionEvidencePackageContext context, CancellationToken cancellationToken = default)
        {
            var packages = new List<ResolutionEvidencePackage>(context.Packages) { _package };
            return Task.FromResult(context with { Packages = packages });
        }
    }

    // A terminal stage that persists the accumulated packages through the FOS-0053
    // evidence package repository abstraction (keyed by each package's query).
    private sealed class PersistStage : ILegalReferenceResolutionEvidencePackageStage
    {
        private readonly ILegalReferenceResolutionEvidencePackageRepository _repository;

        public PersistStage(ILegalReferenceResolutionEvidencePackageRepository repository) => _repository = repository;

        public string Name => "persist";

        public async Task<LegalReferenceResolutionEvidencePackageContext> ExecuteAsync(LegalReferenceResolutionEvidencePackageContext context, CancellationToken cancellationToken = default)
        {
            foreach (var package in context.Packages)
            {
                await _repository.StoreAsync(package.Provenance.Query, package, cancellationToken);
            }

            return context;
        }
    }

    private sealed class SequentialPipeline : ILegalReferenceResolutionEvidencePackagePipeline
    {
        private readonly IReadOnlyList<ILegalReferenceResolutionEvidencePackageStage> _stages;

        public SequentialPipeline(params ILegalReferenceResolutionEvidencePackageStage[] stages) => _stages = stages;

        public async Task<LegalReferenceResolutionEvidencePackageContext> RunAsync(LegalReferenceResolutionEvidencePackageContext context, CancellationToken cancellationToken = default)
        {
            var current = context;
            foreach (var stage in _stages)
            {
                current = await stage.ExecuteAsync(current, cancellationToken);
            }

            return current;
        }
    }

    // Minimal in-memory evidence package repository double (FOS-0053), not production.
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

    private static ResolutionEvidencePackage Package(LegalReference query, FullyQualifiedLegalReference selected)
    {
        var trail = new ResolutionAuditTrail(new[]
        {
            new ResolutionAuditEntry(query, new ResolutionDecision(ResolutionStatus.Resolved, selected), new ResolutionEvidence("matched a single document"), At),
        });
        var result = new ResolutionResult(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(selected) }, Unresolved: null);
        var provenance = new ResolutionProvenance(
            query,
            ResolutionStatus.Resolved,
            new ProvenanceChain(new[] { new ProvenanceStep(new ProvenanceSource("Monitorul Oficial"), "extracted citation") }),
            trail);
        return new ResolutionEvidencePackage(result, trail, provenance);
    }

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    private static ResolutionEvidencePackage PackageA() =>
        Package(QueryParagraph, Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3")));

    private static ResolutionEvidencePackage PackageB() =>
        Package(QueryArticle48, Fq("Legea 227/2015", Seg("Article", "48")));

    private static LegalReferenceResolutionEvidencePackageContext Context() =>
        new(Array.Empty<ResolutionEvidencePackage>());

    [Fact]
    public async Task Stage_executes_and_returns_an_updated_context()
    {
        var package = PackageA();
        ILegalReferenceResolutionEvidencePackageStage stage = new AppendPackageStage(package);
        var context = Context();

        var updated = await stage.ExecuteAsync(context);

        Assert.Equal("append-package", stage.Name);
        Assert.Single(updated.Packages);
        Assert.Same(package, updated.Packages[0]);
        // Original context is unchanged (immutable).
        Assert.Empty(context.Packages);
    }

    [Fact]
    public async Task Pipeline_runs_stages_and_produces_a_context()
    {
        ILegalReferenceResolutionEvidencePackagePipeline pipeline = new SequentialPipeline(
            new AppendPackageStage(PackageA()),
            new AppendPackageStage(PackageB()));

        var result = await pipeline.RunAsync(Context());

        Assert.Equal(2, result.Packages.Count);
    }

    [Fact]
    public async Task Pipeline_can_persist_packages_through_the_repository()
    {
        var repository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();
        var package = PackageA();
        ILegalReferenceResolutionEvidencePackagePipeline pipeline = new SequentialPipeline(
            new AppendPackageStage(package),
            new PersistStage(repository));

        await pipeline.RunAsync(Context());

        var stored = await repository.GetAsync(QueryParagraph);
        Assert.Same(package, stored);
    }
}
