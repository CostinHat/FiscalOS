using System.Linq;
using System.Threading.Tasks;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using FiscalOS.Runtime.LegalReferences.Embedded;
using FiscalOS.Runtime.LegalReferences.Traceability;
using FiscalOS.Runtime.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Embedded_Legal_Reference_Traceability
{
    private static readonly DateTimeOffset At = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private static readonly LegalReference RequestedArticle47 = Address(Seg("Article", "47"));
    private static readonly LegalReference RequestedArticle48 = Address(Seg("Article", "48"));
    private static readonly FullyQualifiedLegalReference CitationArticle47 = Fq("Legea 227/2015", Seg("Article", "47"));
    private static readonly FullyQualifiedLegalReference CitationArticle48 = Fq("Legea 227/2015", Seg("Article", "48"));

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ResolutionResult Ambiguous(params FullyQualifiedLegalReference[] references) =>
        new(ResolutionStatus.Ambiguous, references.Select(reference => new ResolutionCandidate(reference)).ToArray(), Unresolved: null);

    [Fact]
    public async Task Valid_request_preserves_trimmed_correlation_id()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        await repository.StoreAsync(RequestedArticle47, Resolved(CitationArticle47));
        var feature = FeatureWith(repository);

        var response = await feature.ResolveAsync(new EmbeddedLegalReferenceRequest(RequestedArticle47, " corr-1 ", IncludeTraceability: true));

        Assert.Equal("corr-1", response.CorrelationId);
        Assert.Equal("corr-1", response.TraceabilitySummary!.CorrelationId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Blank_correlation_id_is_rejected_at_embedded_boundary(string correlationId)
    {
        var feature = FeatureWith(new InMemoryLegalReferenceResolutionRepository());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            feature.ResolveAsync(new EmbeddedLegalReferenceRequest(RequestedArticle47, correlationId, IncludeTraceability: true)));
    }

    [Fact]
    public async Task Requested_reference_identity_is_preserved_from_request()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        var requested = Address(Seg("Article", "999"));
        await repository.StoreAsync(requested, Ambiguous(CitationArticle47, CitationArticle48));
        var feature = FeatureWith(repository);

        var response = await feature.ResolveAsync(new EmbeddedLegalReferenceRequest(requested, "corr-2", IncludeTraceability: true));

        Assert.Equal(EmbeddedLegalReferenceStatus.Ambiguous, response.Status);
        Assert.Equal("Article 999", response.TraceabilitySummary!.RequestedReference.Display);
        Assert.Null(response.Citation);
        Assert.Null(response.TraceabilitySummary.Citation);
    }

    [Fact]
    public async Task Traceability_summary_is_omitted_when_not_requested()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        await repository.StoreAsync(RequestedArticle47, Resolved(CitationArticle47));
        var feature = FeatureWith(repository);

        var response = await feature.ResolveAsync(new EmbeddedLegalReferenceRequest(RequestedArticle47, "corr-3", IncludeTraceability: false));

        Assert.Null(response.TraceabilitySummary);
        Assert.Equal("corr-3", response.CorrelationId);
    }

    [Fact]
    public async Task Traceability_summary_is_included_when_requested()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        await repository.StoreAsync(RequestedArticle47, Resolved(CitationArticle47));
        var feature = FeatureWith(repository);

        var response = await feature.ResolveAsync(new EmbeddedLegalReferenceRequest(RequestedArticle47, "corr-4", IncludeTraceability: true));

        Assert.NotNull(response.TraceabilitySummary);
        Assert.Equal("Article 47", response.TraceabilitySummary!.RequestedReference.Display);
    }

    [Fact]
    public async Task Resolved_outcome_includes_response_and_traceability_citations()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        await repository.StoreAsync(RequestedArticle47, Resolved(CitationArticle47));
        var feature = FeatureWith(repository);

        var response = await feature.ResolveAsync(new EmbeddedLegalReferenceRequest(RequestedArticle47, "corr-5", IncludeTraceability: true));

        Assert.Equal(EmbeddedLegalReferenceStatus.Resolved, response.Status);
        Assert.NotNull(response.Answer);
        Assert.NotNull(response.Citation);
        Assert.NotNull(response.TraceabilitySummary!.Citation);
        Assert.Equal("Legea 227/2015: Article 47", response.Citation!.Display);
        Assert.Equal(response.Citation.Display, response.TraceabilitySummary.Citation!.Display);
    }

    [Fact]
    public async Task Ambiguous_outcome_has_no_selected_citation()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        await repository.StoreAsync(RequestedArticle47, Ambiguous(CitationArticle47, CitationArticle48));
        var feature = FeatureWith(repository);

        var response = await feature.ResolveAsync(new EmbeddedLegalReferenceRequest(RequestedArticle47, "corr-6", IncludeTraceability: true));

        Assert.Equal(EmbeddedLegalReferenceStatus.Ambiguous, response.Status);
        Assert.Null(response.Answer);
        Assert.Null(response.Citation);
        Assert.Null(response.TraceabilitySummary!.Citation);
    }

    [Fact]
    public async Task Unresolved_outcome_has_no_selected_citation()
    {
        var feature = FeatureWith(new InMemoryLegalReferenceResolutionRepository());

        var response = await feature.ResolveAsync(new EmbeddedLegalReferenceRequest(RequestedArticle48, "corr-7", IncludeTraceability: true));

        Assert.Equal(EmbeddedLegalReferenceStatus.Unresolved, response.Status);
        Assert.Null(response.Answer);
        Assert.Null(response.Citation);
        Assert.Null(response.TraceabilitySummary!.Citation);
    }

    [Fact]
    public void Embedded_response_does_not_expose_internal_resolution_artifacts()
    {
        var forbiddenTypeNames = new[]
        {
            nameof(ResolutionEvidencePackage),
            nameof(ResolutionAuditTrail),
            nameof(ResolutionAuditEntry),
            nameof(ResolutionEvidence),
            nameof(ResolutionProvenance),
            nameof(ProvenanceChain),
            nameof(ProvenanceStep),
            "Repository",
            "Pipeline",
        };
        var exposedPropertyTypeNames = typeof(EmbeddedLegalReferenceResponse)
            .GetProperties()
            .Select(property => property.PropertyType.Name)
            .ToArray();

        foreach (var forbiddenTypeName in forbiddenTypeNames)
        {
            Assert.DoesNotContain(forbiddenTypeName, exposedPropertyTypeNames);
        }
    }

    private static EmbeddedLegalReferenceFeature FeatureWith(ILegalReferenceResolutionRepository resolutionRepository)
    {
        ILegalReferenceResolutionEngine resolutionEngine = new LegalReferenceResolutionEngine(
            new LegalReferenceResolutionPipeline(
                new RepositoryLegalReferenceResolutionStage(resolutionRepository)));
        ILegalReferenceResolutionAuditEngine auditEngine = new LegalReferenceResolutionAuditEngine(
            new LegalReferenceResolutionAuditPipeline(
                new PersistResolutionAuditTrailStage(new InMemoryLegalReferenceResolutionAuditRepository())),
            () => At);
        ILegalReferenceResolutionProvenanceEngine provenanceEngine = new LegalReferenceResolutionProvenanceEngine(
            new LegalReferenceResolutionProvenancePipeline(
                new PersistResolutionProvenanceStage(new InMemoryLegalReferenceResolutionProvenanceRepository())));
        ILegalReferenceResolutionEvidencePackageEngine packageEngine = new LegalReferenceResolutionEvidencePackageEngine(
            new LegalReferenceResolutionEvidencePackagePipeline(
                new PersistResolutionEvidencePackageStage(new InMemoryLegalReferenceResolutionEvidencePackageRepository())));
        var runtime = new LegalReferenceResolutionRuntime(
            resolutionEngine,
            auditEngine,
            provenanceEngine,
            packageEngine,
            new ResolutionEvidencePackageComposer());

        return new EmbeddedLegalReferenceFeature(runtime, new LegalReferenceTraceabilityProjector());
    }
}
