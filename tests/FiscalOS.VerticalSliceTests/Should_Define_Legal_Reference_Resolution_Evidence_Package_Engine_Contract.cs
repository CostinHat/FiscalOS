using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Evidence_Package_Engine_Contract
{
    // Test-only doubles validating the contract shape and semantics. Not
    // production implementations (no packaging algorithm, pipeline
    // implementation, repository implementation, or AI/NLP), per FOS-0055 scope.
    // The engine double is composed over a FOS-0054 pipeline whose stage persists
    // via a FOS-0053 repository, to exercise the full layering.

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

    // FOS-0055 engine double: a facade that runs packages through a FOS-0054
    // pipeline. It does not construct, persist, or merge packages.
    private sealed class PipelineBackedEvidencePackageEngine : ILegalReferenceResolutionEvidencePackageEngine
    {
        private readonly ILegalReferenceResolutionEvidencePackagePipeline _pipeline;

        public PipelineBackedEvidencePackageEngine(ILegalReferenceResolutionEvidencePackagePipeline pipeline) => _pipeline = pipeline;

        public async Task<IReadOnlyList<ResolutionEvidencePackage>> ProcessAsync(IReadOnlyList<ResolutionEvidencePackage> packages, CancellationToken cancellationToken = default)
        {
            var context = new LegalReferenceResolutionEvidencePackageContext(packages);
            var processed = await _pipeline.RunAsync(context, cancellationToken);
            return processed.Packages;
        }
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

    private static ILegalReferenceResolutionEvidencePackageEngine EngineWith(InMemoryLegalReferenceResolutionEvidencePackageRepository repository) =>
        new PipelineBackedEvidencePackageEngine(new SequentialPipeline(new PersistStage(repository)));

    [Fact]
    public async Task Engine_processes_a_single_package()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionEvidencePackageRepository());
        var package = PackageA();

        var processed = await engine.ProcessAsync(new[] { package });

        Assert.Single(processed);
        Assert.Same(package, processed[0]);
    }

    [Fact]
    public async Task Engine_preserves_one_package_per_input_package()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionEvidencePackageRepository());
        var packages = new[] { PackageA(), PackageB() };

        var processed = await engine.ProcessAsync(packages);

        Assert.Equal(2, processed.Count);
        Assert.Same(packages[0], processed[0]);
        Assert.Same(packages[1], processed[1]);
    }

    [Fact]
    public async Task Engine_returns_no_packages_for_no_input_packages()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionEvidencePackageRepository());

        var processed = await engine.ProcessAsync(Array.Empty<ResolutionEvidencePackage>());

        Assert.Empty(processed);
    }

    [Fact]
    public async Task Engine_can_process_packages_through_the_repository_layer()
    {
        var repository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();
        var engine = EngineWith(repository);
        var package = PackageA();

        await engine.ProcessAsync(new[] { package });

        var stored = await repository.GetAsync(QueryParagraph);
        Assert.Same(package, stored);
    }
}
