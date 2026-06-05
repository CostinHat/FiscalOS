using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Legal_Reference_Resolution_Evidence_Package_Implementation
{
    private sealed class RecordingPackageStage : ILegalReferenceResolutionEvidencePackageStage
    {
        private readonly ResolutionEvidencePackage _package;
        private readonly List<string> _executionOrder;

        public RecordingPackageStage(string marker, List<string> executionOrder)
        {
            _package = Package(Address(Seg("Marker", marker)), Fq("Test", Seg("Marker", marker)));
            _executionOrder = executionOrder;
            Name = $"record-package-{marker}";
        }

        public string Name { get; }

        public Task<LegalReferenceResolutionEvidencePackageContext> ExecuteAsync(
            LegalReferenceResolutionEvidencePackageContext context,
            CancellationToken cancellationToken = default)
        {
            _executionOrder.Add(Name);
            return Task.FromResult(context with
            {
                Packages = context.Packages.Concat(new[] { _package }).ToList(),
            });
        }
    }

    private sealed class StubPackagePipeline : ILegalReferenceResolutionEvidencePackagePipeline
    {
        private readonly IReadOnlyList<ResolutionEvidencePackage> _packages;

        public StubPackagePipeline(IReadOnlyList<ResolutionEvidencePackage> packages) => _packages = packages;

        public LegalReferenceResolutionEvidencePackageContext? ReceivedContext { get; private set; }

        public Task<LegalReferenceResolutionEvidencePackageContext> RunAsync(
            LegalReferenceResolutionEvidencePackageContext context,
            CancellationToken cancellationToken = default)
        {
            ReceivedContext = context;
            return Task.FromResult(context with { Packages = _packages });
        }
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

    private static ProvenanceStep Step(string source, string description) =>
        new(new ProvenanceSource(source), description);

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ResolutionResult Ambiguous(params FullyQualifiedLegalReference[] references) =>
        new(ResolutionStatus.Ambiguous, references.Select(reference => new ResolutionCandidate(reference)).ToList(), Unresolved: null);

    private static ResolutionResult Unresolved(LegalReference query, string reason) =>
        new(ResolutionStatus.Unresolved, Array.Empty<ResolutionCandidate>(), new UnresolvedReference(query, reason));

    private static ResolutionAuditEntry Entry(LegalReference query, FullyQualifiedLegalReference selected) =>
        new(query, new ResolutionDecision(ResolutionStatus.Resolved, selected), new ResolutionEvidence("Resolved legal reference."), At);

    private static ResolutionProvenance Provenance(LegalReference query, ResolutionAuditTrail trail) =>
        new(
            query,
            ResolutionStatus.Resolved,
            new ProvenanceChain(new[] { Step("resolution-runtime", "resolved from repository result") }),
            trail);

    private static ResolutionEvidencePackage Package(LegalReference query, FullyQualifiedLegalReference selected)
    {
        var trail = new ResolutionAuditTrail(new[] { Entry(query, selected) });
        return new ResolutionEvidencePackage(Resolved(selected), trail, Provenance(query, trail));
    }

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    [Fact]
    public async Task Evidence_package_pipeline_runs_configured_stages_in_order()
    {
        var executionOrder = new List<string>();
        var pipeline = new LegalReferenceResolutionEvidencePackagePipeline(
            new RecordingPackageStage("first", executionOrder),
            new RecordingPackageStage("second", executionOrder));
        var context = new LegalReferenceResolutionEvidencePackageContext(Array.Empty<ResolutionEvidencePackage>());

        var result = await pipeline.RunAsync(context);

        Assert.Equal(new[] { "record-package-first", "record-package-second" }, executionOrder);
        Assert.Equal(2, result.Packages.Count);
        Assert.Empty(context.Packages);
    }

    [Fact]
    public async Task Persist_stage_stores_packages_without_changing_context()
    {
        var repository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        var package = Package(QueryParagraph, selected);
        var stage = new PersistResolutionEvidencePackageStage(repository);
        var context = new LegalReferenceResolutionEvidencePackageContext(new[] { package });

        var result = await stage.ExecuteAsync(context);

        var stored = await repository.GetAsync(QueryParagraph);
        Assert.Equal("persist-resolution-evidence-package", stage.Name);
        Assert.Same(context, result);
        Assert.Same(package, stored);
    }

    [Fact]
    public async Task Evidence_package_engine_delegates_to_pipeline()
    {
        var selected = Fq("Legea 227/2015", Seg("Article", "48"));
        var processedPackage = Package(QueryArticle48, selected);
        var pipeline = new StubPackagePipeline(new[] { processedPackage });
        var engine = new LegalReferenceResolutionEvidencePackageEngine(pipeline);

        var packages = await engine.ProcessAsync(Array.Empty<ResolutionEvidencePackage>());

        Assert.Single(packages);
        Assert.Same(processedPackage, packages[0]);
        Assert.NotNull(pipeline.ReceivedContext);
        Assert.Empty(pipeline.ReceivedContext!.Packages);
    }

    [Fact]
    public void Composer_composes_result_audit_and_provenance_by_query()
    {
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        var result = Resolved(selected);
        var trail = new ResolutionAuditTrail(new[] { Entry(QueryParagraph, selected) });
        var provenance = Provenance(QueryParagraph, trail);
        var composer = new ResolutionEvidencePackageComposer();

        var packages = composer.Compose(new[] { result }, trail, new[] { provenance });

        Assert.Single(packages);
        Assert.Same(result, packages[0].Result);
        Assert.Same(provenance, packages[0].Provenance);
        Assert.Single(packages[0].AuditTrail.Entries);
        Assert.Same(trail.Entries[0], packages[0].AuditTrail.Entries[0]);
    }

    [Fact]
    public void Composer_derives_ambiguous_query_from_the_first_candidate()
    {
        var first = Fq("Legea 227/2015", Seg("Article", "48"));
        var second = Fq("OUG 1/2020", Seg("Article", "49"));
        var result = Ambiguous(first, second);
        var trail = new ResolutionAuditTrail(new[] { Entry(first.Reference, first) });
        var provenance = Provenance(first.Reference, trail);
        var composer = new ResolutionEvidencePackageComposer();

        var packages = composer.Compose(new[] { result }, trail, new[] { provenance });

        Assert.Single(packages);
        Assert.Equal(first.Reference, packages[0].Provenance.Query);
        Assert.NotEqual(second.Reference, packages[0].Provenance.Query);
    }

    [Fact]
    public void Composer_throws_when_audit_entry_is_missing()
    {
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        var result = Resolved(selected);
        var trail = new ResolutionAuditTrail(Array.Empty<ResolutionAuditEntry>());
        var provenance = Provenance(QueryParagraph, new ResolutionAuditTrail(new[] { Entry(QueryParagraph, selected) }));
        var composer = new ResolutionEvidencePackageComposer();

        var exception = Assert.Throws<InvalidOperationException>(() => composer.Compose(new[] { result }, trail, new[] { provenance }));

        Assert.Contains("No audit entries were found", exception.Message);
    }

    [Fact]
    public void Composer_throws_when_provenance_is_missing()
    {
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        var result = Resolved(selected);
        var trail = new ResolutionAuditTrail(new[] { Entry(QueryParagraph, selected) });
        var composer = new ResolutionEvidencePackageComposer();

        var exception = Assert.Throws<InvalidOperationException>(() => composer.Compose(new[] { result }, trail, Array.Empty<ResolutionProvenance>()));

        Assert.Contains("No provenance was found", exception.Message);
    }

    [Fact]
    public void Composer_throws_when_resolved_result_has_no_selected_candidate()
    {
        var invalid = new ResolutionResult(ResolutionStatus.Resolved, Array.Empty<ResolutionCandidate>(), Unresolved: null);
        var composer = new ResolutionEvidencePackageComposer();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            composer.Compose(new[] { invalid }, new ResolutionAuditTrail(Array.Empty<ResolutionAuditEntry>()), Array.Empty<ResolutionProvenance>()));

        Assert.Contains("resolved result must contain exactly one selected candidate", exception.Message);
    }

    [Fact]
    public void Composer_throws_when_ambiguous_result_has_no_candidates()
    {
        var invalid = new ResolutionResult(ResolutionStatus.Ambiguous, Array.Empty<ResolutionCandidate>(), Unresolved: null);
        var composer = new ResolutionEvidencePackageComposer();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            composer.Compose(new[] { invalid }, new ResolutionAuditTrail(Array.Empty<ResolutionAuditEntry>()), Array.Empty<ResolutionProvenance>()));

        Assert.Contains("ambiguous result must contain at least one candidate", exception.Message);
    }

    [Fact]
    public void Composer_throws_when_unresolved_result_has_no_unresolved_details()
    {
        var invalid = new ResolutionResult(ResolutionStatus.Unresolved, Array.Empty<ResolutionCandidate>(), Unresolved: null);
        var composer = new ResolutionEvidencePackageComposer();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            composer.Compose(new[] { invalid }, new ResolutionAuditTrail(Array.Empty<ResolutionAuditEntry>()), Array.Empty<ResolutionProvenance>()));

        Assert.Contains("unresolved result must contain unresolved reference details", exception.Message);
    }
}
